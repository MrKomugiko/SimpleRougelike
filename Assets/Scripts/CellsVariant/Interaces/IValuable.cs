internal interface IValuable
{
    int GoldValue { get; set; }

    void Pick(out bool result);
}