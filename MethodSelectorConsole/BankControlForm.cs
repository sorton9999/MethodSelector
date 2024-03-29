﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MethodSelectorConsole
{
    public partial class BankControlForm : Form
    {
        static Bank _bank = null;
        public BankControlForm(string name, Bank bank)
        {
            _bank = bank;
            InitializeComponent();
            bankControl1.Vm.ActiveAccountName = name;
            bankControl1.acctListView.ItemsSource = _bank.AccountDetailsList.AccountDetailsList;
            this.FormClosing += BankControlForm_FormClosing;
        }

        private void BankControlForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            bankControl1.CloseWindow();
        }

        public static Bank Bank
        {
            get { return _bank; }
            private set { _bank = value; }
        }
    }
}
