﻿namespace Nenter.Data.SqlAdapter
{
    /// <summary>
    ///     Config for SqlAdapterConfig
    /// </summary>
    public class SqlAdapterConfig
    {
        /// <summary>
        ///     Type Sql provider
        /// </summary>
        public SqlProvider SqlProvider { get; set; }

        /// <summary>
        ///     Use quotation marks for TableName and ColumnName
        /// </summary>
        public bool UseQuotationMarks { get; set; }
    }
}