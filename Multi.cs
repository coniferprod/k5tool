namespace k5tool
{
    public enum BankName
    {
        BankA,
        BankB,
        BankC,
        BankD
    }

    public struct Track
    {
        public BankName Bank;
        public byte PatchNumber;

    }
    public struct Multi
    {
        public Track[] Tracks;
    }    
}
