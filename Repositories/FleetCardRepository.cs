using ImportBankFleetCardAPI.DTOs;
using ImportBankFleetCardAPI.Models;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Text;

namespace ImportBankFleetCardAPI.Repositories
{
    public class FleetCardRepository : IFleetCardRepository
    {
        private readonly string _connectionString;

        public FleetCardRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Oracle") 
                ?? throw new InvalidOperationException("Oracle Connection String ('Oracle') is not configured in appsettings.json.");
        }

        public async Task UpsertFleetCardAsync(FleetCard card)
        {
            await using var connection = new OracleConnection(_connectionString);
            await using var command = new OracleCommand("FLEET_CARD_API_PKG.SP_UPSERT_FLEET_CARD", connection) 
                { CommandType = CommandType.StoredProcedure };

            command.Parameters.Add("p_CardNumber", OracleDbType.Varchar2, card.CardNumber, ParameterDirection.Input);
            command.Parameters.Add("p_DriverName", OracleDbType.Varchar2, card.DriverName, ParameterDirection.Input);
            command.Parameters.Add("p_VehiclePlate", OracleDbType.Varchar2, card.VehiclePlate, ParameterDirection.Input);
            command.Parameters.Add("p_ExpiryDate", OracleDbType.Date, card.ExpiryDate, ParameterDirection.Input);
            command.Parameters.Add("p_Status", OracleDbType.Varchar2, card.Status, ParameterDirection.Input);
            command.Parameters.Add("p_CardType", OracleDbType.Varchar2, card.CardType, ParameterDirection.Input);
            command.Parameters.Add("p_CardHolderName", OracleDbType.Varchar2, card.CardHolderName, ParameterDirection.Input);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task<(string Status, string ErrorMessage)> ProcessTransactionRowAsync(Dictionary<string, string?> rowData)
        {
            await using var connection = new OracleConnection(_connectionString);
            await using var command = new OracleCommand("FLEET_CARD_API_PKG.SP_PROCESS_TRANSACTION_ROW", connection) 
                { CommandType = CommandType.StoredProcedure };

            // Input Parameters
            command.Parameters.Add("p_card_number", rowData.GetValueOrDefault("CardNumber"));
            command.Parameters.Add("p_plate_number", rowData.GetValueOrDefault("PlateNumber"));
            command.Parameters.Add("p_invoice_no", rowData.GetValueOrDefault("InvoiceNo"));
            command.Parameters.Add("p_transaction_date", rowData.GetValueOrDefault("TransactionDateTime")); // ส่ง TransactionDateTime
            command.Parameters.Add("p_station_name", rowData.GetValueOrDefault("StationName"));
            command.Parameters.Add("p_product_name", rowData.GetValueOrDefault("ProductName"));
            command.Parameters.Add("p_quantity", rowData.GetValueOrDefault("QuantityLitre")); // ใช้ QuantityLitre
            command.Parameters.Add("p_unit_price", rowData.GetValueOrDefault("UnitPrice"));
            command.Parameters.Add("p_total_amount", rowData.GetValueOrDefault("TotalAmount"));

            // Output Parameters
            var pStatus = new OracleParameter("p_status", OracleDbType.Varchar2, 50, null, ParameterDirection.Output);
            var pErrorMessage = new OracleParameter("p_error_message", OracleDbType.Varchar2, 4000, null, ParameterDirection.Output);
            command.Parameters.Add(pStatus);
            command.Parameters.Add(pErrorMessage);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            return (pStatus.Value.ToString()!, pErrorMessage.Value.ToString() ?? "");
        }

        public async Task<FleetCard?> GetFleetCardByNumberAsync(string cardNumber)
        {
            await using var connection = new OracleConnection(_connectionString);
            await using var command = new OracleCommand("SELECT * FROM EFM_FED.FLEET_CARDS WHERE CARD_NUMBER = :p_CardNumber", connection);
            command.Parameters.Add("p_CardNumber", cardNumber);

            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapReaderToFleetCard(reader);
            }
            return null;
        }

        public async Task<IEnumerable<FleetCard>> GetAllFleetCardsAsync(int pageNumber, int pageSize)
        {
            var cards = new List<FleetCard>();
            await using var connection = new OracleConnection(_connectionString);
            var sql = @"
                SELECT * FROM (
                    SELECT a.*, ROWNUM rnum FROM (
                        SELECT * FROM EFM_FED.FLEET_CARDS ORDER BY CARD_NUMBER
                    ) a WHERE ROWNUM <= :p_end_row
                ) WHERE rnum >= :p_start_row";
            await using var command = new OracleCommand(sql, connection);
            command.Parameters.Add("p_end_row", pageNumber * pageSize);
            command.Parameters.Add("p_start_row", ((pageNumber - 1) * pageSize) + 1);

            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                cards.Add(MapReaderToFleetCard(reader));
            }
            return cards;
        }

        public async Task<IEnumerable<FleetCardTransaction>> SearchTransactionsAsync(TransactionSearchCriteria criteria)
        {
            var transactions = new List<FleetCardTransaction>();
            var sqlBuilder = new StringBuilder("SELECT * FROM EFM_FED.FLEET_CARD_TRANSACTIONS WHERE 1=1");
            var parameters = new List<OracleParameter>();

            if (!string.IsNullOrEmpty(criteria.CardNumber))
            {
                sqlBuilder.Append(" AND CARD_NUMBER = :p_CardNumber");
                parameters.Add(new OracleParameter("p_CardNumber", criteria.CardNumber));
            }
            // ... เพิ่มเงื่อนไขอื่นๆ ตาม criteria ...

            sqlBuilder.Append(" ORDER BY TRANSACTION_DATE DESC");

            await using var connection = new OracleConnection(_connectionString);
            await using var command = new OracleCommand(sqlBuilder.ToString(), connection);
            if(parameters.Any()) command.Parameters.AddRange(parameters.ToArray());

            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                transactions.Add(MapReaderToTransaction(reader));
            }
            return transactions;
        }

        // --- Private Helper Methods ---
        private FleetCard MapReaderToFleetCard(IDataReader reader)
        {
            return new FleetCard
            {
                CardNumber = reader["CARD_NUMBER"]?.ToString(),
                DriverName = reader["DRIVER_NAME"]?.ToString(),
                VehiclePlate = reader["VEHICLE_PLATE"]?.ToString(),
                ExpiryDate = reader["EXPIRY_DATE"] == DBNull.Value ? default : Convert.ToDateTime(reader["EXPIRY_DATE"]),
                Status = reader["STATUS"]?.ToString(),
                CardType = reader["CARD_TYPE"]?.ToString(),
                CardHolderName = reader["CARD_HOLDER_NAME"]?.ToString()
            };
        }

        private FleetCardTransaction MapReaderToTransaction(IDataReader reader)
        {
            return new FleetCardTransaction
            {
                TransactionId = Convert.ToInt64(reader["TRANSACTION_ID"]),
                CardNumber = reader["CARD_NUMBER"]?.ToString(),
                PlateNumber = reader["PLATE_NUMBER"]?.ToString(),
                TransactionDate = Convert.ToDateTime(reader["TRANSACTION_DATE"]),
                MerchantId = reader["MERCHANT_ID"]?.ToString(),
                TaxId = reader["TAX_ID"]?.ToString(),
                StationName = reader["STATION_NAME"]?.ToString(),
                InvoiceNo = reader["INVOICE_NO"]?.ToString(),
                ProductName = reader["PRODUCT_NAME"]?.ToString(),
                Quantity = reader["QUANTITY"] == DBNull.Value ? null : Convert.ToDecimal(reader["QUANTITY"]),
                TotalAmount = reader["TOTAL_AMOUNT"] == DBNull.Value ? null : Convert.ToDecimal(reader["TOTAL_AMOUNT"]),
                Odometer = reader["ODOMETER"] == DBNull.Value ? null : Convert.ToInt64(reader["ODOMETER"]),
                Status = reader["STATUS"]?.ToString()
                // ... Map คอลัมน์อื่นๆ ตามต้องการ ...
            };
        }
    }
}