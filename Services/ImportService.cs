using CsvHelper;
using CsvHelper.Configuration;
using ImportBankFleetCardAPI.Config;
using ImportBankFleetCardAPI.DTOs;
using ImportBankFleetCardAPI.Logging;
using ImportBankFleetCardAPI.Repositories;
using OfficeOpenXml;
using System.Globalization;

namespace ImportBankFleetCardAPI.Services
{
    public class ImportService : IImportService
    {
        private readonly IFleetCardRepository _repository;
        private readonly ILoggingService _loggingService;
        private readonly IConfigService _configService;

        public ImportService(IFleetCardRepository repository, ILoggingService loggingService, IConfigService configService)
        {
            _repository = repository;
            _loggingService = loggingService;
            _configService = configService;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task<ImportResult<Dictionary<string, string?>>> ImportTransactionsAsync(IFormFile file)
        {
            // 1. วิเคราะห์ชื่อไฟล์เพื่อหา Template ที่ต้องใช้
            string determinedTemplateName = DetermineTemplateNameFromFileName(file.FileName);

            // 2. ดึงการตั้งค่า (Config) จากฐานข้อมูล
            var config = await _configService.GetTemplateConfigAsync(determinedTemplateName);
            if (!config.Any())
            {
                throw new Exception($"Configuration for template '{determinedTemplateName}' (derived from filename '{file.FileName}') not found in database.");
            }

            // 3. เลือกว่าจะใช้วิธีประมวลผลแบบไหนตามนามสกุลไฟล์
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return extension switch
            {
                ".csv" => await ProcessCsvFileAsync(file, config),
                ".xlsx" or ".xls" => await ProcessXlsxReportAsync(file, config),
                _ => throw new NotSupportedException("File type not supported. Please upload a .csv, .xls or .xlsx file.")
            };
        }

        /// <summary>
        /// เมธอดสำหรับประมวลผลไฟล์ CSV ทีละแถว
        /// </summary>
        private async Task<ImportResult<Dictionary<string, string?>>> ProcessCsvFileAsync(IFormFile file, IReadOnlyDictionary<string, TemplateFieldConfig> config)
        {
            var result = new ImportResult<Dictionary<string, string?>>();
            int rowNumber = 1; // เริ่มนับจากแถว Header

            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true });
            csv.Read();
            csv.ReadHeader();

            while (csv.Read())
            {
                rowNumber++;
                var rawRecordForLog = csv.Context.Parser.RawRecord;
                var rowData = new Dictionary<string, string?>();
                try
                {
                    // Map ข้อมูลดิบจากแถวปัจจุบันตาม Config
                    var dateStr = config.ContainsKey("TransactionDate") ? csv.GetField(config["TransactionDate"].SourceColumnName) : null;
                    var timeStr = config.ContainsKey("TransactionTime") ? csv.GetField(config["TransactionTime"].SourceColumnName) : "00:00:00";
                    rowData["TransactionDateTime"] = $"{dateStr} {timeStr}";
                    
                    foreach (var field in config)
                    {
                        if (field.Key != "TransactionDate" && field.Key != "TransactionTime")
                        {
                            rowData[field.Key] = csv.GetField(field.Value.SourceColumnName);
                        }
                    }

                    // ส่งข้อมูลดิบไปให้ Repository ประมวลผล
                    var (status, errorMessage) = await _repository.ProcessTransactionRowAsync(rowData);

                    if (status == "COMPLETED")
                    {
                        result.SuccessDetails.Add(rowData);
                    }
                    else
                    {
                        throw new Exception(errorMessage); // โยน Error ที่ได้รับจาก Stored Procedure
                    }
                }
                catch (Exception ex)
                {
                    var failureDetail = new ImportFailureDetail { RowNumber = rowNumber, RawData = rawRecordForLog, ErrorMessage = ex.Message };
                    result.FailureDetails.Add(failureDetail);
                    await _loggingService.LogErrorAsync(ex, "Failed to process CSV row.", $"File: {file.FileName}, Row: {rowNumber}");
                }
            }
            result.TotalRowsInFile = rowNumber - 1;
            result.SuccessfulRows = result.SuccessDetails.Count;
            result.FailedRows = result.FailureDetails.Count;
            return result;
        }

        /// <summary>
        /// เมธอดสำหรับประมวลผลไฟล์ Excel Report ที่มีโครงสร้างซับซ้อน
        /// </summary>
        private async Task<ImportResult<Dictionary<string, string?>>> ProcessXlsxReportAsync(IFormFile file, IReadOnlyDictionary<string, TemplateFieldConfig> config)
        {
            var result = new ImportResult<Dictionary<string, string?>>();
            using var package = new ExcelPackage(file.OpenReadStream());
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null) return result;

            string currentCardNumber = "", currentPlateNumber = "";
            
            for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
            {
                var rawDataForLog = string.Join(" | ", Enumerable.Range(1, worksheet.Dimension.End.Column).Select(col => worksheet.Cells[row, col].Text));
                try
                {
                    var cellA_Text = worksheet.Cells[row, 1]?.Text.Trim() ?? "";

                    // ตรวจสอบว่าเป็นแถว Header ของกลุ่มข้อมูลหรือไม่
                    if (cellA_Text.StartsWith("Card no."))
                    {
                        currentCardNumber = GetValueFromConfig(worksheet, row, config, "CardNumberHeader")?.Split(':').LastOrDefault()?.Trim() ?? "";
                        currentPlateNumber = GetValueFromConfig(worksheet, row, config, "PlateNumberHeader")?.Split(':').LastOrDefault()?.Trim() ?? "";
                    }
                    // ตรวจสอบว่าเป็นแถวข้อมูล Transaction หรือไม่ (โดยดูจากคอลัมน์แรกว่าเป็นวันที่หรือไม่)
                    else if (DateTime.TryParseExact(cellA_Text, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                    {
                        var rowData = new Dictionary<string, string?>
                        {
                            ["CardNumber"] = currentCardNumber,
                            ["PlateNumber"] = currentPlateNumber
                        };
                        
                        foreach(var field in config)
                        {
                             if(!field.Key.EndsWith("Header"))
                             {
                                 rowData[field.Key] = GetValueFromConfig(worksheet, row, config, field.Key);
                             }
                        }
                        
                        var (status, errorMessage) = await _repository.ProcessTransactionRowAsync(rowData);
                        if (status == "COMPLETED")
                        {
                            result.SuccessDetails.Add(rowData);
                        }
                        else
                        {
                            throw new Exception(errorMessage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var failureDetail = new ImportFailureDetail { RowNumber = row, RawData = rawDataForLog, ErrorMessage = ex.Message };
                    result.FailureDetails.Add(failureDetail);
                    await _loggingService.LogErrorAsync(ex, "Failed to process XLSX report row.", $"File: {file.FileName}, Row: {row}");
                }
            }

            result.TotalRowsInFile = result.SuccessDetails.Count + result.FailureDetails.Count;
            result.SuccessfulRows = result.SuccessDetails.Count;
            result.FailedRows = result.FailureDetails.Count;
            return result;
        }

        /// <summary>
        /// เมธอด Helper สำหรับวิเคราะห์ชื่อไฟล์เพื่อหา Template
        /// </summary>
        private string DetermineTemplateNameFromFileName(string fileName)
        {
            var upperFileName = fileName.ToUpperInvariant();

            if (upperFileName.StartsWith("VAT_"))
            {
                return "VAT_CSV_TEMPLATE";
            }
            if (upperFileName.StartsWith("NOVAT_"))
            {
                return "NOVAT_CSV_TEMPLATE";
            }
            if (upperFileName.Contains("REPORT"))
            {
                 return "ORPT_MONTHLY_REPORT";
            }
            
            throw new ArgumentException("Invalid filename format. Cannot determine template type from filename.");
        }
        
        /// <summary>
        /// เมธอด Helper สำหรับดึงค่าจาก Cell ใน Excel ตาม Config
        /// </summary>
        private string? GetValueFromConfig(ExcelWorksheet worksheet, int row, IReadOnlyDictionary<string, TemplateFieldConfig> config, string fieldName)
        {
            if (config.TryGetValue(fieldName, out var fieldConfig) && fieldConfig.SourceColumnIndex.HasValue)
            {
                return worksheet.Cells[row, fieldConfig.SourceColumnIndex.Value].Text;
            }
            return null;
        }
    }
}