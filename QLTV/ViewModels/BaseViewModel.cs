using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QLTV.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        private bool _isValidationActive;
        private string _errorSummary;

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual string this[string columnName] => string.Empty;

        public string Error => null;

        public bool IsValidationActive
        {
            get => _isValidationActive;
            protected set => SetProperty(ref _isValidationActive, value);
        }

        public string ErrorSummary
        {
            get => _errorSummary;
            set
            {
                if (SetProperty(ref _errorSummary, value))
                    OnPropertyChanged(nameof(HasErrorSummary));
            }
        }

        public bool HasErrorSummary => !string.IsNullOrEmpty(_errorSummary);

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void ResetValidation()
        {
            IsValidationActive = false;
            ErrorSummary = string.Empty;
        }

        protected void ActivateValidation()
        {
            IsValidationActive = true;
            OnPropertyChanged(string.Empty);
        }

        protected string GetValidationSummary(params string[] propertyNames)
        {
            ActivateValidation();

            var errors = new List<string>();
            foreach (var name in propertyNames)
            {
                var message = this[name];
                if (!string.IsNullOrEmpty(message))
                    errors.Add(message);
            }

            return string.Join(System.Environment.NewLine, errors);
        }
    }
}
