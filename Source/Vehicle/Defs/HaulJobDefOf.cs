using RimWorld;
using Verse;

namespace ToolsForHaul.JobDefs
{
    [DefOf]
    public static class HaulJobDefOf
    {

        public static readonly JobDef HaulWithBackpack = DefDatabase<JobDef>.GetNamed("HaulWithBackpack");

        // public static readonly JobDef Board = DefDatabase<JobDef>.GetNamed("Board");

        public static readonly JobDef PutInBackpackSlot = DefDatabase<JobDef>.GetNamed("PutInBackpackSlot");

        public static readonly JobDef PutInToolbeltSlot = DefDatabase<JobDef>.GetNamed("PutInToolbeltSlot");
    }
}
