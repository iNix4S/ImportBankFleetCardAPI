﻿using ImportBankFleetCardAPI.Config;
using Microsoft.Extensions.Caching.Memory;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;

namespace ImportBankFleetCardAPI.Config
{
    public class DbConfigService : IConfigService
    {
        private readonly Services.IDatabaseConnectionService _oracleConnectionService;
        private readonly IMemoryCache _cache;

        /// <summary>
        /* Constructor รับ OracleConnectionService และ IMemoryCache สำหรับทำ Caching */
        /// </summary>
        public DbConfigService(Services.IDatabaseConnectionService oracleConnectionService, IMemoryCache cache)
        {
            _oracleConnectionService = oracleConnectionService;
            _cache = cache;
        }

        /// <summary>
        /* ดึงข้อมูลการตั้งค่าของ Template ตามชื่อที่ระบุ */
        /// </summary>
        public async Task<IReadOnlyDictionary<string, TemplateFieldConfig>> GetTemplateConfigAsync(string templateName)
        {
            string cacheKey = $"TemplateConfig_{templateName}";

            // 1. ตรวจสอบใน Cache ก่อน ถ้ามีข้อมูลอยู่แล้วให้ดึงจาก Cache ไปใช้ได้เลย
            if (_cache.TryGetValue(cacheKey, out IReadOnlyDictionary<string, TemplateFieldConfig>? cachedConfig))
            {
                return cachedConfig ?? new Dictionary<string, TemplateFieldConfig>();
            }

            // 2. ถ้าใน Cache ไม่มีข้อมูล ให้ไปดึงจากฐานข้อมูล
            var config = new Dictionary<string, TemplateFieldConfig>();
            await using var connection = await _oracleConnectionService.GetOpenConnectionAsync();
            var sql = "SELECT FIELD_NAME, SOURCE_COLUMN_NAME, SOURCE_COLUMN_INDEX, IS_REQUIRED FROM EFM_FED.FLEET_CARD_IMPORT_CONFIGS WHERE TEMPLATE_NAME = :p_TemplateName";

            await using var command = new Oracle.ManagedDataAccess.Client.OracleCommand(sql, connection);
            command.Parameters.Add("p_TemplateName", Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2, templateName, System.Data.ParameterDirection.Input);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var fieldConfig = new TemplateFieldConfig
                {
                    FieldName = reader["FIELD_NAME"].ToString()!,
                    SourceColumnName = reader["SOURCE_COLUMN_NAME"]?.ToString(),
                    SourceColumnIndex = reader["SOURCE_COLUMN_INDEX"] == DBNull.Value ? null : Convert.ToInt32(reader["SOURCE_COLUMN_INDEX"]),
                    IsRequired = Convert.ToInt32(reader["IS_REQUIRED"]) == 1
                };

                if (!string.IsNullOrEmpty(fieldConfig.FieldName))
                {
                    config[fieldConfig.FieldName] = fieldConfig;
                }
            }

            // 3. นำข้อมูลที่ได้จากฐานข้อมูลไปเก็บไว้ใน Cache สำหรับการใช้งานครั้งต่อไป
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromHours(1)); // ให้ข้อมูลใน Cache อยู่ได้ 1 ชั่วโมง

            _cache.Set(cacheKey, config, cacheEntryOptions);
            
            return config;
        }

        /// <summary>
        /* ล้าง Cache ของ Template ที่ระบุ */
        /// </summary>
        public Task ClearCacheForTemplateAsync(string templateName)
        {
            string cacheKey = $"TemplateConfig_{templateName}";
            _cache.Remove(cacheKey);
            return Task.CompletedTask;
        }
    }
}