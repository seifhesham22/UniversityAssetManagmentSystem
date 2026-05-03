using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Abstractions.Policy
{
    public static class Policies
    {
        public const string SuperAdminOnly = nameof(SuperAdminOnly);
        public const string AssetManagerOnly = nameof(AssetManagerOnly);
        public const string DepartmentManagerOnly = nameof(DepartmentManagerOnly);
        public const string MaintainerOnly = nameof(MaintainerOnly);
        public const string TeacherOnly = nameof(TeacherOnly);
        public const string StudentOnly = nameof(StudentOnly);

        public const string CanViewRoomDesign = nameof(CanViewRoomDesign);
        public const string CanManageTicket = nameof(CanManageTicket);
        public const string CanReportIssue = nameof(CanReportIssue);
        public const string CanConfirmFix = nameof(CanConfirmFix);
        public const string CanCheckChecklist = nameof(CanCheckChecklist);
        public const string CanCloseRoom = nameof(CanCloseRoom);
        public const string CanDesignRoom = nameof(CanDesignRoom);
        public const string CanManageAssets = nameof(CanManageAssets);
        public const string CanSubmitInspection = nameof(CanSubmitInspection);
        public const string CanViewFaculties = nameof(CanViewFaculties);
    }
}