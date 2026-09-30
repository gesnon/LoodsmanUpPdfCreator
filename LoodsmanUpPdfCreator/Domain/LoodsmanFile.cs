using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LoodsmanUpPdfCreator.Domain
{
    public class LoodsmanFile : INotifyPropertyChanged
    {
        public LoodsmanFile()
        {

        }
        public string _name { get; set; }        
        public string _localName { get; set; }
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
        public string LocalName
        {
            get => _localName;
            set
            {
                if (_localName == value)
                    return;

                _localName = value;

                OnPropertyChanged(nameof(LocalName));
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

        public override string ToString()
        {
            return $"{Name} {Id} {LocalName}";
        }
    }
}
