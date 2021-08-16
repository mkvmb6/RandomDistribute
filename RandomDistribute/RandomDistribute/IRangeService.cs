namespace RandomDistribute
{
    public interface IRangeService
    {
        MyRange NumberRange { get; set; }

        MyRange NextNumberRange { get; }

        MyRange GetRange();

        void UpdateNextNumberRange();
    }
}