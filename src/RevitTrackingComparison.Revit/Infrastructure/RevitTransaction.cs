using Autodesk.Revit.DB;

namespace RevitTrackingComparison.Revit.Infrastructure;

public static class RevitTransaction
{
    public static bool Run(Document document, string name, Action<Document> edit)
    {
        using var transaction = new Transaction(document, name);

        var options = transaction.GetFailureHandlingOptions();
        options.SetFailuresPreprocessor(new FailureSuppressor());
        options.SetClearAfterRollback(true);
        transaction.SetFailureHandlingOptions(options);

        transaction.Start();
        try
        {
            edit(document);
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, $"Transaction '{name}' failed; rolling back.");
            transaction.RollBack();
            return false;
        }

        return transaction.Commit() == TransactionStatus.Committed;
    }
}