using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace Navend.Core.Data.EfCore;
public class NoLockCommandInterceptor : DbCommandInterceptor
{
    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        command.CommandText = RewriteWithNoLock(command.CommandText);
        return base.ReaderExecuting(command, eventData, result);
    }

    private string RewriteWithNoLock(string sql)
    {
        // En basit haliyle tüm FROM ifadelerine NOLOCK ekler
        return System.Text.RegularExpressions.Regex.Replace(
            sql,
            @"FROM\s+(\[?\w+\]?(?:\s+\[?\w+\]?)?)",
            "FROM $1 WITH (NOLOCK)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
    }
}
