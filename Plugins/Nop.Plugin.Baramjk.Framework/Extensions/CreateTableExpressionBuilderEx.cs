using FluentMigrator.Builders.Create.Table;

namespace Nop.Plugin.Baramjk.Framework.Extensions
{
    public static class CreateTableExpressionBuilderEx
    {
        public static ICreateTableColumnOptionOrWithColumnSyntax Id(this CreateTableExpressionBuilder table)
        {
            return table.WithColumn("Id").AsInt32().PrimaryKey().Identity();
        }

        public static ICreateTableColumnOptionOrWithColumnSyntax Int32Nullable(
            this ICreateTableColumnOptionOrWithColumnSyntax table, string name)
        {
            return table.WithColumn(name).AsInt32().Nullable();
        }

        public static ICreateTableColumnOptionOrWithColumnSyntax Int32(
            this ICreateTableColumnOptionOrWithColumnSyntax table, string name)
        {
            return table.WithColumn(name).AsInt32().NotNullable();
        }

        public static ICreateTableColumnOptionOrWithColumnSyntax StringNullable(
            this ICreateTableColumnOptionOrWithColumnSyntax table, string name, int size = 50)
        {
            return table.WithColumn(name).AsString(size).Nullable();
        }

        public static ICreateTableColumnOptionOrWithColumnSyntax String(
            this ICreateTableColumnOptionOrWithColumnSyntax table, string name, int size = 50)
        {
            return table.WithColumn(name).AsString(size).NotNullable();
        }

        public static ICreateTableColumnOptionOrWithColumnSyntax AsDateTime(
            this ICreateTableColumnOptionOrWithColumnSyntax table, string name)
        {
            return table.WithColumn(name).AsDateTime().NotNullable();
        }

        public static ICreateTableColumnOptionOrWithColumnSyntax AsDateTimeNullable(
            this ICreateTableColumnOptionOrWithColumnSyntax table, string name)
        {
            return table.WithColumn(name).AsDateTime().Nullable();
        }
    }
}