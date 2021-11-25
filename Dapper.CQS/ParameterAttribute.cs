using System;
using System.Data;

namespace Dapper.CQS
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ParameterAttribute : Attribute
    {
        public string? Name { get; set; }
        public DbType? DbType { get; set; }
        public ParameterDirection? Direction { get; set; }
        public int? Size { get; set; }

        public ParameterAttribute()
        {
        }

        public ParameterAttribute(string? name, DbType? dbType, ParameterDirection? direction, int? size)
        {
            Name = name;
            DbType = dbType;
            Direction = direction;
            Size = size;
        }
    }
}
