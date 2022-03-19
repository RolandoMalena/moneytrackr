using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using System.Text;

namespace MoneyTrackr.Data.Migrations.Helpers
{
    public static class MigrationBuilderExtensions
    {
        public static OperationBuilder<SqlOperation> Insert(this MigrationBuilder builder, string TableName, string[] Columns, object[] ValueSet)
        {
            return builder.Insert(TableName, Columns, new object[][] { ValueSet });
        }

        public static OperationBuilder<SqlOperation> Insert(this MigrationBuilder migrationBuilder, string TableName, string[] Columns, object[][] ValueSets)
        {
            StringBuilder sb = new();
            foreach (object[] set in ValueSets)
            {
                sb.Append($@"INSERT INTO {Constants.Data.Schema}.""{TableName}""(");
                sb.Append($@"""{string.Join(@""", """, Columns)}"") VALUES(");
                sb.Append(ParseValues(set));
                sb.AppendLine(");");
            }
            return migrationBuilder.Sql(sb.ToString());
        }

        private static string ParseValues(object[] Values)
        {
            string[] parsedValues = Values.Select(v => ParseValue(v)).ToArray();
            return string.Join(", ", parsedValues);
        }

        private static string ParseValue(object Value) 
        {
            if (Value == null)
                return "NULL";

            if (Value is string)
                return $"'{Value}'";

            return Value.ToString() ?? "NULL";
        }
    }
}
