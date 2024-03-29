﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MethodSelectorConsole
{
    public class AccountNameViewModel : ViewModelBase
    {
        //private string name = null;
        //private string errorString = "";
        CommonClasses.AccountNameModel model = new CommonClasses.AccountNameModel();

        public string ActiveAccountName
        {
            get { return model.name; }
            set
            {
                model.name = value;
                NotifyPropertyChanged();
            }
        }

        public string ErrorString
        {
            get { return model.errorString; }
            set
            {
                model.errorString = value;
                NotifyPropertyChanged();
            }
        }
    }

}
