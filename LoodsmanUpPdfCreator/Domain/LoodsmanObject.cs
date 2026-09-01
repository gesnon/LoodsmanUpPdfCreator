using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoodsmanUpPdfCreator.Domain
{
    public sealed class LoodsmanObject: INotifyPropertyChanged
    {

        public LoodsmanObject()
        {
            
        }

        public string _name { get; set; }
        public string _type { get; set; }
        public int _id { get; set; }


        public string Name
        {
            get => _name;
            set
            {
                if (_name == value)
                    return;

                _name = value;

                OnPropertyChanged(nameof(Name));
            }
        }
        public string Type
        {
            get => _type;
            set
            {
                if (_type == value)
                    return;

                _type = value;

                OnPropertyChanged(nameof(Type));
            }
        }
        public int Id
        {
            get => _id;
            set
            {
                if (_id == value)
                    return;

                _id = value;

                OnPropertyChanged(nameof(Id));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(
             this,
             new PropertyChangedEventArgs(propertyName));
        }
    }
}
