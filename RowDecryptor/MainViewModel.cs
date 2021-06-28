using System;
using System.ComponentModel;
using System.Text.Json;
using RowDecryptor.extensions;

namespace RowDecryptor
{
    public class MainViewModel : MathCore.ViewModels.ViewModel
    {
        #region InputRow : string - Входная строка

        /// <summary>Входная строка</summary>
        private string _InputRow;

        /// <summary>Входная строка</summary>
        public string InputRow { get => _InputRow; set => Set(ref _InputRow, value); }

        #endregion

        #region Secret : string - Строка шифрования/дешифрирования

        /// <summary>Строка шифрования/дешифрирования</summary>
        private string _Secret;

        /// <summary>Строка шифрования/дешифрирования</summary>
        public string Secret { get => _Secret; set => Set(ref _Secret, value); }

        #endregion

        #region ResultRow : string - Результирующая строка

        /// <summary>Результирующая строка</summary>
        private string _ResultRow;

        /// <summary>Результирующая строка</summary>
        public string ResultRow { get => _ResultRow; set => Set(ref _ResultRow, value); }

        #endregion

        #region IsEncrypted : bool - зашифровать или расшифровать

        /// <summary>зашифровать или расшифровать</summary>
        private bool _IsEncrypted;

        /// <summary>зашифровать или расшифровать</summary>
        public bool IsEncrypted { get => _IsEncrypted; set => Set(ref _IsEncrypted, value); }

        #endregion

        public MainViewModel()
        {
            void Updater(object s, PropertyChangedEventArgs e) => UpdateRow();
            this.SubscribeTo(nameof(InputRow), Updater);
            this.SubscribeTo(nameof(Secret), Updater);
            this.SubscribeTo(nameof(IsEncrypted), Updater);
        }

        private void UpdateRow()
        {
            try
            {
                ResultRow = IsEncrypted ? InputRow.DecryptRow<string>(Secret) : InputRow.EncryptToRow(Secret);;
            }
            catch (Exception e)
            {
                ResultRow = $"ERROR: {e.Message}";
            }
        }
    }
}
