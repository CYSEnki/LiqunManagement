using LiqunManagement.ViewModels;
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
                               //order = null,
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
                                   order = null,
                                   text = bank.BankName + bank.BankCode,
                                   id = bank.BankCode,
                               }).Distinct();
                    break;
                case "branches":
                    ///20231003修正，中華郵政的分布代碼全部皆為0021，為了可以區分不同支部，將所有0021加上-Number做區分
                    bankDDL = (from bank in formdb.Bank.Where(x => x.RootCheck == true && x.BankCode == bankcode)
                               select new DDLViewModel
                               {
                                   order = null,
                                   text = bank.BranchName + (bankcode != "700" ? bank.BranchCode : "0021"),
                                   id = bank.BranchCode,
                               }).Distinct();
                    break;

            }


            var formmodel = new FormViewModels();
            formmodel.ddllist = bankDDL;
            return formmodel;
        }
        #endregion

        #region 找到部門下拉選單
        public MemberRegisterViewModel GetDeptDDL(string divcode)
        {
            IEnumerable<DDLViewModel> deptDDL = new List<DDLViewModel>();

            deptDDL = (from deptdb in Memberdb.Department
                       select new DDLViewModel
                       {
                           order = null,
                           text = deptdb.DivFullName,
                           id = deptdb.DivCode,
                       }).Distinct();
            var model = new MemberRegisterViewModel
            {
                ddllist = deptDDL,
            };

            return model;
        }
        #endregion
        
        #region 找到主管下拉選單
        public MemberRegisterViewModel GetManager(string divcode)
        {
            IEnumerable<DDLViewModel> managerddl = new List<DDLViewModel>();

            managerddl = (from db in Memberdb.EmployeeData.Where(x => x.DivCode == divcode)
                       join members in Memberdb.Members on db.Account equals members.Account
                       select new DDLViewModel
                       {
                           order = null,
                           text = members.Name,
                           id = members.Account,
                       }).Distinct();
            var model = new MemberRegisterViewModel
            {
                ddllist = managerddl,
            };

            return model;
        }
        #endregion

        #region 找到段/小段下拉選單
        /// <summary>
        /// 
        /// </summary>
        /// <param name="CityName">縣市名稱</param>
        /// <param name="DistrictName">鄉鎮市區名稱</param>
        /// <param name="ExcerptName">段/小段名稱</param>
        /// <param name="ExcerptCode">段名代碼</param>
        /// <returns></returns>
        public FormViewModels GetExcerptDDL(string CityName, string DistrictName, string ExcerptName, string ExcerptType)
        {
            //var citycodelist = formdb.Region.Select(x => x.CityCode).Distinct();
            IEnumerable<DDLViewModel> ddl = new List<DDLViewModel>();

            switch (ExcerptType)
            {
                case "Excerpt":
                    //段
                    ddl = (from ex in formdb.Excerpt.Where(x => x.CityName.Trim() == CityName && x.DistrictName.Trim() == DistrictName)
                           select new DDLViewModel
                           {
                               text = ex.Excerpt1,
                               id = ex.Excerpt1,
                           }).Distinct().OrderBy(x => x.id);
                    break;

                default:
                    //小段
                    ddl = (from ex in formdb.Excerpt.Where(x => x.CityName.Trim() == CityName && x.DistrictName.Trim() == DistrictName && x.Excerpt1.Trim() == ExcerptName)
                           select new DDLViewModel
                           {
                               text = String.IsNullOrEmpty(ex.ExcerptShort) ? "" : ex.ExcerptShort,
                               id = String.IsNullOrEmpty(ex.ExcerptShort) ? "" : ex.ExcerptShort,
                           }).Distinct().OrderBy(x => x.id);
                    break;
            }
            var ddd = ddl.ToList();

            var formmodel = new FormViewModels();
            formmodel.ddllist = ddl;
            return formmodel;
        }
        #endregion

    }
}