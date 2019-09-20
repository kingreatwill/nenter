select * from student
order by sno 
offset ((@pageIndex-1)*@pageSize) rows
fetch next @pageSize rows only;

Annotations 
IValidatableObject.cs
Validator.cs
KeyAttribute.cs
UIHintAttribute.cs
TimestampAttribute.cs
ConcurrencyCheckAttribute.cs 乐观并发检查
ScaffoldColumnAttribute.cs 脚手架
DisplayAttribute.cs  提供一个通用特性，使您可以为实体分部类的类型和成员指定可本地化的字符串。
DisplayColumnAttribute.cs 将所引用的表中显示的列指定为外键列。
DisplayFormatAttribute.cs 指定 ASP.NET 动态数据如何显示数据字段以及如何设置数据字段的格式。
EditableAttribute.cs 
MetadataTypeAttribute.cs 指定要与数据模型类关联的元数据类。
ValidationAttribute.cs
    CustomValidationAttribute.cs
    CompareAttribute.cs
    StringLengthAttribute.cs
    RequiredAttribute.cs
    MaxLengthAttribute.cs
    MinLengthAttribute.cs
    RangeAttribute.cs
    RegularExpressionAttribute.cs
    DataTypeAttribute.cs 
        UrlAttribute.cs 指定数据字段值是url
        CreditCardAttribute.cs 指定数据字段值是信用卡号
        EmailAddressAttribute.cs 
        EnumDataTypeAttribute.cs 使枚举能够映射到数据列。
        FileExtensionsAttribute.cs 文件扩展名验证
        PhoneAttribute.cs 
        
Schema
    TableAttribute.cs
    NotMappedAttribute.cs
    InversePropertyAttribute.cs
    ForeignKeyAttribute.cs
    ComplexTypeAttribute.cs 复杂类型
    ColumnAttribute.cs
    DatabaseGeneratedAttribute.cs
        DatabaseGeneratedOption.cs  
            None
            Identity
            Computed 

 MSSQL   SELECT SCOPE_IDENTITY() AS
 
  MySQL   ; SELECT CONVERT(LAST_INSERT_ID(), SIGNED INTEGER) AS
  
  PostgreSQL     RETURNING