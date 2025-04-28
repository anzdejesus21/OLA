using MudBlazor;

namespace OLA.Services
{
    public interface ISnackbarService
    {
        void Success(string message);
        void Error(string message);
        void Warning(string message);
        void Info(string message);
    }

    public class SnackbarService : ISnackbarService
    {
        private readonly ISnackbar Snackbar;

        public SnackbarService(ISnackbar Snackbar)
        {
            this.Snackbar = Snackbar;
            this.Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopEnd;
        }

        public void Error(string message)
        {
            this.Snackbar.Add(message, Severity.Error);
        }

        public void Info(string message)
        {
            this.Snackbar.Add(message, Severity.Info);
        }

        public void Success(string message)
        {
            this.Snackbar.Add(message, Severity.Success);
        }

        public void Warning(string message)
        {
            this.Snackbar.Add(message, Severity.Warning);
        }
    }
}
