﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
 using Dapper;

 namespace Nenter.Dapper.Linq.Helpers
{
    internal interface ISqlWriter<TData>
    {
        Type SelectType{ get; set; }
        
        bool NotOperater{ get; set; }
        
        int TopCount{ get; set; }
        
        bool IsDistinct{ get; set; }

        DynamicParameters Parameters { get; set; }

        string Sql { get; }


        void WriteOrder(string name, bool descending);

        void WriteJoin(string joinToTableName, string joinToTableIdentifier, string primaryJoinColumn,
            string secondaryJoinColumn);

        void Write(object value);

        void Parameter(object val);

        void AliasName(string aliasName);

        void ColumnName(string columnName);

        void IsNull();

        void IsNullFunction();

        void Like();

        void In();

        void Operator();

        void Boolean(bool op);

        void OpenBrace();

        void CloseBrace();

        void WhiteSpace();

        void Delimiter();

        void LikePrefix();

        void LikeSuffix();

        void EmptyString();
    }
}
