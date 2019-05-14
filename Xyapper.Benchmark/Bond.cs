using System;
using System.Data;

namespace Xyapper.Benchmark
{
    public class Bond : ICustomDeserialized
    {
        public System.DateTime? Date { get; set; }
        public System.Double? FaceValue { get; set; }
        public System.Double? TraderPrice { get; set; }
        public System.Double? Accrued { get; set; }
        public System.Double? AccruedT0 { get; set; }
        public System.Double? AccruedAmount { get; set; }
        public System.String MaturityDate { get; set; }
        public System.String ISIN { get; set; }
        public System.String ShortName { get; set; }
        public System.Int32? Bond_Id { get; set; }
        public System.String HolidayCountry { get; set; }
        public System.String BondCcy { get; set; }
        public System.String Name { get; set; }
        public System.Double? Nominal { get; set; }
        public System.String InstrumentCode { get; set; }
        public System.DateTime? BookDate { get; set; }
        public System.DateTime? DeleteDate { get; set; }

        public string ISIN2 { get; set; }

        public string ISIN3 { get; set; }

        [ColumnMapping("test2")]
        public bool Test { get; set; }

        public void Deserialize(IDataRecord record)
        {


            object date = record["Date"];
            Date = date == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(date);

            object faceValue = record["FaceValue"];
            FaceValue = faceValue == DBNull.Value ? null : (double?)Convert.ToDouble(faceValue);

            object TraderPrice = record["TraderPrice"];
            TraderPrice = TraderPrice == DBNull.Value ? null : (double?)Convert.ToDouble(TraderPrice);

            object Accrued = record["Accrued"];
            Accrued = Accrued == DBNull.Value ? null : (double?)Convert.ToDouble(Accrued);

            object AccruedT0 = record["AccruedT0"];
            AccruedT0 = AccruedT0 == DBNull.Value ? null : (double?)Convert.ToDouble(AccruedT0);

            object AccruedAmount = record["AccruedAmount"];
            AccruedAmount = AccruedAmount == DBNull.Value ? null : (double?)Convert.ToDouble(AccruedAmount);

            object MaturityDate = record["MaturityDate"];
            MaturityDate = MaturityDate == DBNull.Value ? null : MaturityDate.ToString();

            object ISIN = record["ISIN"];
            ISIN = ISIN == DBNull.Value ? null : ISIN.ToString();

            object ShortName = record["ShortName"];
            ShortName = ShortName == DBNull.Value ? null : ShortName.ToString();

            object Bond_Id = record["Bond_Id"];
            Bond_Id = Bond_Id == DBNull.Value ? null : (int?)Convert.ToInt32(Bond_Id);

            object HolidayCountry = record["HolidayCountry"];
            HolidayCountry = HolidayCountry == DBNull.Value ? null : HolidayCountry.ToString();

            object BondCcy = record["BondCcy"];
            BondCcy = BondCcy == DBNull.Value ? null : BondCcy.ToString();

            object Name = record["Name"];
            Name = Name == DBNull.Value ? null : Name.ToString();

            object Nominal = record["Nominal"];
            Nominal = Nominal == DBNull.Value ? null : (double?)Convert.ToDouble(Nominal);

            object InstrumentCode = record["InstrumentCode"];
            Name = Name == DBNull.Value ? null : Name.ToString();

            object BookDate = record["BookDate"];
            BookDate = BookDate == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(BookDate);

            object DeleteDate = record["DeleteDate"];
            DeleteDate = DeleteDate == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(DeleteDate);
        }
    }
}
