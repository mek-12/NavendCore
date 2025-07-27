using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Navend.Core.Data;

public interface IEntity<Tkey> {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    Tkey Id {get;set;}
}