using System.Collections.Generic;
using System.Linq;
using Soneta.Business;
using Soneta.Core;
using Soneta.Types;

namespace Geekout.AiWSoneta.UI;

public class StreszczenieParams(Context context) : ContextBase(context)
{
    [Caption("System zewnętrzny")] [Required]
    public SystemZewn SystemZewn { get; set; }

    private IEnumerable<SystemZewn> _listSystemZewn;
    private LookupInfo.EnumerableItem _enumerableSysZewn;

    private IEnumerable<SystemZewn> ListSystemZewn
        => _listSystemZewn
            ??= CoreModule.GetInstance(Context.Session).SystemyZewn.WgTyp[TypSystemuZewn.SerwisAI].Where(x => !x.Blokada);
    internal LookupInfo.EnumerableItem GetCollection()
        => new(CoreModule.GetInstance(Context.Session).SystemyZewn.TableName, ListSystemZewn, nameof(SystemZewn.Symbol));

    public LookupInfo.EnumerableItem GetListSystemZewn() => _enumerableSysZewn ??= GetCollection();
}
