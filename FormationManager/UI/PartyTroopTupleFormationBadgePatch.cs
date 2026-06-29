using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace FormationManager.UI
{
    /// <summary>
    /// Injects a formation badge into each party troop row's main strip (right side).
    /// Click  = cycle formation assignment (none → I … VIII → none)
    /// Right-click = clear
    /// </summary>
    [PrefabExtension("PartyTroopTuple", "descendant::ButtonWidget[@Id='LockButton']")]
    public class PartyTroopTupleFormationBadgePatch : PrefabExtensionInsertPatch
    {
        public override InsertType Type => InsertType.Append;

        [PrefabExtensionFileName(true)]
        public string MyXmlFile => "PartyTroopTupleFormationBadge";
    }
}

