namespace UAMS.Campus.Models
{
    public sealed class FacultyBuilding
    {
        public Guid FacultyId { get; private set; }
        public Guid BuildingId { get; private set; }
        public Faculty Faculty { get; private set; } = null!;
        public Building Building { get; private set; } = null!;

        private FacultyBuilding() { }
        public FacultyBuilding(Guid facultyId, Guid buildingId)
        {
            FacultyId = facultyId;
            BuildingId = buildingId;
        }
    }
}