using System.ComponentModel.DataAnnotations;

namespace Navend.Core.Data;

public interface IEntity<Tkey> {
    [Key]
    Tkey Id {get;set;}
}