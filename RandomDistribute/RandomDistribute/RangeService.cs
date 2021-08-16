using System.Data.SqlClient;

namespace RandomDistribute
{
    public class RangeService : IRangeService
    {
        public MyRange NumberRange { get; set; }

        public MyRange NextNumberRange { get; private set; }

        private readonly long myRangeSize;
        private readonly string myConnectionString;

        public RangeService(long rangeSize, string connectionString)
        {
            myRangeSize = rangeSize;
            myConnectionString = connectionString;
            NumberRange = GetRange();
        }

        public MyRange GetRange()
        {
            using var connection = new SqlConnection(myConnectionString);
            using var cmd = new SqlCommand("sp_GetRange", connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("rangeSize", myRangeSize);
            connection.Open();
            var startRange = (long)cmd.ExecuteScalar();
            connection.Close();

            return new MyRange { Start = startRange, Current = startRange, End = startRange + myRangeSize - 1 };
        }

        public void UpdateNextNumberRange()
        {
            NextNumberRange = GetRange();
        }
    }
}
