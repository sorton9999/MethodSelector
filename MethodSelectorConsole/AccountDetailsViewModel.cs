﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MethodSelectorConsole
{
    public class AccountDetailsViewModel : ViewModelBase
    {
        //private string accountName = "nobody";
        //private AccountType accountType = AccountType.UNINIT;
        //private string accountId = "0";
        //private float accountBalance = 0F;
        //private bool addBtn = false;
        CommonClasses.AccountDetailsModel model = new CommonClasses.AccountDetailsModel();

        public string AccountName
        {
            get { return model.accountName; }
            set
            {
                model.accountName = value;
                NotifyPropertyChanged();
            }
        }
        public CommonClasses.AccountType Type
        {
            get { return model.accountType; }
            set
            {
                model.accountType = value;
                NotifyPropertyChanged();
            }
        }
        public string AccountId
        {
            get { return model.accountId; }
            set
            {
                model.accountId = value;
                NotifyPropertyChanged();
            }
        }
        public float Balance
        {
            get { return model.accountBalance; }
            set
            {
                model.accountBalance = value;
                NotifyPropertyChanged();
            }
        }

        public bool AddBtn
        {
            get { return model.addBtn; }
            set
            {
                model.addBtn = value;
                NotifyPropertyChanged();
            }
        }
    }
}
