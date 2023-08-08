﻿using LiqunManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LiqunManagement.Services
{
    public class DDLServices : BaseService
    {

        #region 找到縣市街道下拉選單
        public FormViewModels GetRegionDDL(string regioncode)
        {
            //var citycodelist = formdb.Region.Select(x => x.CityCode).Distinct();
            IEnumerable<DDLViewModel> ddl = new List<DDLViewModel>();
            var codelength = regioncode.Length;
            switch (codelength)
            {
                case 0:
                    ddl = (from reg in formdb.Region
                           select new DDLViewModel
                           {
                               order = reg.CityOrder,
                               text = reg.City,
                               id = reg.CityCode,
                           }).Distinct().OrderBy(x => x.order);
                    break;
                case 2:
                    ddl = (from reg in formdb.Region
                           where reg.CityCode == regioncode
                           select new DDLViewModel
                           {
                               text = reg.District,
                               id = reg.DistrictCode,
                           }).Distinct().OrderBy(x => x.id);
                    break;
                case 4:
                    ddl = (from reg in formdb.Region
                           where reg.DistrictCode == regioncode
                           select new DDLViewModel
                           {
                               text = reg.Road,
                               id = reg.RoadCode,
                           }).Distinct().OrderBy(x => x.id);
                    break;
            }
            var formmodel = new FormViewModels();
            formmodel.ddllist = ddl;
            return formmodel;
        }
        #endregion

        #region 找到銀行下拉選單 
        public FormViewModels GetBankDDL(string bankcode, string banktype)
        {
            //var citycodelist = formdb.Region.Select(x => x.CityCode).Distinct();
            IEnumerable<DDLViewModel> bankDDL = new List<DDLViewModel>();
            var codelength = bankcode.Length;
            switch (banktype)
            {
                case "bank":
                    bankDDL = (from bank in formdb.Bank.Where(x => x.RootCheck == true)
                               select new DDLViewModel
                               {
                                   order = 1,
                                   text = bank.BankName + bank.BankCode,
                                   id = bank.BankCode,
                               }).Distinct();
                    break;
                case "branches":
                    bankDDL = (from bank in formdb.Bank.Where(x => x.RootCheck == true && x.BankCode == bankcode)
                               select new DDLViewModel
                               {
                                   order = 1,
                                   text = bank.BranchName + bank.BranchCode,
                                   id = bank.BranchCode,
                               }).Distinct();
                    break;

            }


            var formmodel = new FormViewModels();
            formmodel.ddllist = bankDDL;
            return formmodel;
        }
        #endregion


    }
}