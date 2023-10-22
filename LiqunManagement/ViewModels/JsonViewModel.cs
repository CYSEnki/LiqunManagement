using LiqunManagement.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using Newtonsoft.Json;

namespace LiqunManagement.ViewModels
{
    public class JsonViewModel
    {
        public class MyData
        {
            [JsonProperty("case_content")]
            public CaseContent CaseContent { get; set; }

            [JsonProperty("case_build")]
            public List<CaseBuild> CaseBuild { get; set; }

            [JsonIgnore]
            [JsonProperty("case_car")]
            public List<CaseCar> CaseCar { get; set; }
        }
        public class CaseContent
        {
            [JsonProperty("case_type")]
            public string CaseType { get; set; }

            [JsonProperty("case_no")]
            public string CaseNo { get; set; }

            [JsonProperty("p1ma_caseSeq")]
            public string P1maCaseSeq { get; set; }

            [JsonProperty("case_x45")]
            public string CaseX45 { get; set; }

            [JsonProperty("case_x45c")]
            public string CaseX45c { get; set; }

            [JsonProperty("case_x46")]
            public string CaseX46 { get; set; }

            [JsonProperty("case_x46c")]
            public string CaseX46c { get; set; }

            [JsonProperty("case_unit")]
            public string CaseUnit { get; set; }

            [JsonProperty("case_unitc")]
            public string CaseUnitc { get; set; }

            [JsonProperty("case_kind")]
            public string CaseKind { get; set; }

            [JsonProperty("caseflag3")]
            public string CaseFlag3 { get; set; }

            [JsonProperty("caseflag5")]
            public string CaseFlag5 { get; set; }

            [JsonProperty("apply_name")]
            public string ApplyName { get; set; }

            [JsonProperty("apply_idNo")]
            public string ApplyIdNo { get; set; }

            [JsonProperty("apply_tel")]
            public string ApplyTel { get; set; }

            [JsonProperty("apply_mail")]
            public string ApplyMail { get; set; }

            [JsonProperty("apply_x45")]
            public string ApplyX45 { get; set; }

            [JsonProperty("apply_x45c")]
            public string ApplyX45c { get; set; }

            [JsonProperty("apply_x46")]
            public string ApplyX46 { get; set; }

            [JsonProperty("apply_x46c")]
            public string ApplyX46c { get; set; }

            [JsonProperty("apply_addr")]
            public string ApplyAddr { get; set; }

            [JsonProperty("agents_name")]
            public string AgentsName { get; set; }

            [JsonProperty("agents_idNo")]
            public string AgentsIdNo { get; set; }

            [JsonProperty("agents_tel")]
            public string AgentsTel { get; set; }

            [JsonProperty("agents_mail")]
            public string AgentsMail { get; set; }

            [JsonProperty("agents_x45")]
            public string AgentsX45 { get; set; }

            [JsonProperty("agents_x45c")]
            public string AgentsX45c { get; set; }

            [JsonProperty("agents_x46")]
            public string AgentsX46 { get; set; }

            [JsonProperty("agents_x46c")]
            public string AgentsX46c { get; set; }

            [JsonProperty("agents_addr")]
            public string AgentsAddr { get; set; }

            [JsonProperty("right_name")]
            public string RightName { get; set; }

            [JsonProperty("right_idNo")]
            public string RightIdNo { get; set; }

            [JsonProperty("right_tel")]
            public string RightTel { get; set; }

            [JsonProperty("right_mail")]
            public string RightMail { get; set; }

            [JsonProperty("right_x45")]
            public string RightX45 { get; set; }

            [JsonProperty("right_x45c")]
            public string RightX45c { get; set; }

            [JsonProperty("right_x46")]
            public string RightX46 { get; set; }

            [JsonProperty("right_x46c")]
            public string RightX46c { get; set; }

            [JsonProperty("right_addr")]
            public string RightAddr { get; set; }

            [JsonProperty("p1ma_build7")]
            public string P1maBuild7 { get; set; }

            [JsonProperty("p1ma_build7c")]
            public string P1maBuild7c { get; set; }

            [JsonProperty("p1ma_build_rentType")]
            public string P1maBuildRentType { get; set; }

            [JsonProperty("p1ma_build_rentTypec")]
            public string P1maBuildRentTypec { get; set; }

            [JsonProperty("p1ma_cntalid")]
            public string P1maCntalid { get; set; }

            [JsonProperty("p1ma_cntdbid")]
            public string P1maCntdbid { get; set; }

            [JsonProperty("p1ma_cntroom")]
            public string P1maCntroom { get; set; }

            [JsonProperty("land_x45")]
            public string LandX45 { get; set; }

            [JsonProperty("land_x45c")]
            public string LandX45c { get; set; }

            [JsonProperty("land_x46")]
            public string LandX46 { get; set; }

            [JsonProperty("land_x46c")]
            public string LandX46c { get; set; }

            [JsonProperty("p1ma_dd09")]
            public string P1maDd09 { get; set; }

            [JsonProperty("p1ma_build9")]
            public string P1maBuild9 { get; set; }

            [JsonProperty("p1ma_build10_1")]
            public string P1maBuild10_1 { get; set; }

            [JsonProperty("p1ma_build10_1c")]
            public string P1maBuild10_1c { get; set; }

            [JsonProperty("p1ma_build10_1Text")]
            public string P1maBuild10_1Text { get; set; }

            [JsonProperty("build_10_all")]
            public string Build10All { get; set; }

            [JsonProperty("p1ma_build1")]
            public string P1maBuild1 { get; set; }

            [JsonProperty("p1ma_build2")]
            public string P1maBuild2 { get; set; }

            [JsonProperty("p1ma_build3")]
            public string P1maBuild3 { get; set; }

            [JsonProperty("p1ma_build4")]
            public string P1maBuild4 { get; set; }

            [JsonProperty("p1ma_build5")]
            public string P1maBuild5 { get; set; }

            [JsonProperty("p1ma_build5c")]
            public string P1maBuild5c { get; set; }

            [JsonProperty("p1ma_build5Text")]
            public string P1maBuild5Text { get; set; }

            [JsonProperty("equipmentA")]
            public string EquipmentA { get; set; }

            [JsonProperty("equipmentA_qty")]
            public string EquipmentAQty { get; set; }

            [JsonProperty("equipmentB")]
            public string EquipmentB { get; set; }

            [JsonProperty("equipmentC")]
            public string EquipmentC { get; set; }

            [JsonProperty("equipmentD")]
            public string EquipmentD { get; set; }

            [JsonProperty("equipmentE")]
            public string EquipmentE { get; set; }

            [JsonProperty("equipmentF")]
            public string EquipmentF { get; set; }
            [JsonProperty("equipmentG")]
            public string EquipmentG { get; set; }

            [JsonProperty("equipmentH")]
            public string EquipmentH { get; set; }

            [JsonProperty("equipmentI")]
            public string EquipmentI { get; set; }

            [JsonProperty("equipmentZ")]
            public string EquipmentZ { get; set; }

            [JsonProperty("rental_date_s")]
            public string RentalDateS { get; set; }

            [JsonProperty("rental_date_e")]
            public string RentalDateE { get; set; }

            [JsonProperty("p1ma_date")]
            public string P1maDate { get; set; }

            [JsonProperty("p1ma_manage")]
            public string P1maManage { get; set; }

            [JsonProperty("p1ma_manager")]
            public string P1maManager { get; set; }

            [JsonProperty("p1ma_typec_1")]
            public string P1maTypec1 { get; set; }

            [JsonProperty("rentalService")]
            public string RentalService { get; set; }

            [JsonProperty("rentalServicec")]
            public string RentalServicec { get; set; }

            [JsonProperty("p1ma_totprice")]
            public string P1maTotprice { get; set; }

            [JsonProperty("p1ma_parkflag")]
            public string P1maParkflag { get; set; }

            [JsonProperty("p1ma_cntpark")]
            public string P1maCntpark { get; set; }

            [JsonProperty("p1ma_parkflag2")]
            public string P1maParkflag2 { get; set; }

            [JsonProperty("p1ma_parkprice")]
            public string P1maParkprice { get; set; }

            [JsonProperty("note0101")]
            public string Note0101 { get; set; }

            [JsonProperty("price0101")]
            public string Price0101 { get; set; }

            [JsonProperty("note0201")]
            public string Note0201 { get; set; }

            [JsonProperty("note0202")]
            public string Note0202 { get; set; }

            [JsonProperty("note0202_A")]
            public string Note0202A { get; set; }

            [JsonProperty("note0202_B")]
            public string Note0202B { get; set; }

            [JsonProperty("note0202_C")]
            public string Note0202C { get; set; }

            [JsonProperty("note0202_D")]
            public string Note0202D { get; set; }

            [JsonProperty("note0202_E")]
            public string Note0202E { get; set; }

            [JsonProperty("note0202_F")]
            public string Note0202F { get; set; }

            [JsonProperty("desc0202_F")]
            public string Desc0202F { get; set; }

            [JsonProperty("note0301")]
            public string Note0301 { get; set; }

            [JsonProperty("desc0301")]
            public string Desc0301 { get; set; }

            [JsonProperty("note0302")]
            public string Note0302 { get; set; }

            [JsonProperty("note0303")]
            public string Note0303 { get; set; }

            [JsonProperty("note0304")]
            public string Note0304 { get; set; }

            [JsonProperty("note0305")]
            public string Note0305 { get; set; }

            [JsonProperty("desc0305")]
            public string Desc0305 { get; set; }

            [JsonProperty("note0401")]
            public string Note0401 { get; set; }

            [JsonProperty("note0402")]
            public string Note0402 { get; set; }

            [JsonProperty("note0403")]
            public string Note0403 { get; set; }

            [JsonProperty("note0404")]
            public string Note0404 { get; set; }

            [JsonProperty("note0501")]
            public string Note0501 { get; set; }

            [JsonProperty("note0502")]
            public string Note0502 { get; set; }

            [JsonProperty("note0503")]
            public string Note0503 { get; set; }

            [JsonProperty("note0504")]
            public string Note0504 { get; set; }

            [JsonProperty("note0505")]
            public string Note0505 { get; set; }

            [JsonProperty("p1ma_note")]
            public string P1maNote { get; set; }
        }
        public class CaseBuild
        {
            [JsonProperty("build_seq")]
            public string BuildSeq { get; set; }

            [JsonProperty("build_x48")]
            public string BuildX48 { get; set; }

            [JsonProperty("build_x48c")]
            public string BuildX48c { get; set; }

            [JsonProperty("build_x45")]
            public string BuildX45 { get; set; }

            [JsonProperty("build_x45c")]
            public string BuildX45c { get; set; }

            [JsonProperty("build_x46")]
            public string BuildX46 { get; set; }

            [JsonProperty("build_x46c")]
            public string BuildX46c { get; set; }

            [JsonProperty("build_no")]
            public string BuildNo { get; set; }

            [JsonProperty("build_area")]
            public string BuildArea { get; set; }
        }
        public class CaseCar
        {
            [JsonProperty("car_seq")]
            public string CarSeq { get; set; }

            [JsonProperty("car_type")]
            public string CarType { get; set; }

            [JsonProperty("car_typec")]
            public string CarTypec { get; set; }

            [JsonProperty("car_typeText")]
            public string CarTypeText { get; set; }

            [JsonProperty("car_price")]
            public string CarPrice { get; set; }

            [JsonProperty("car_area")]
            public string CarArea { get; set; }

            [JsonProperty("car_floor")]
            public string CarFloor { get; set; }

            [JsonProperty("car_floorc")]
            public string CarFloorc { get; set; }
        }

    }
}