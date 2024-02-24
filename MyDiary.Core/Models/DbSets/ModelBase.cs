using System.ComponentModel.DataAnnotations;

namespace MyDiary.Core.Models
{
    public abstract class ModelBase
    {
        [Key]
        public int Id { get; set; }

        public bool IsDeleted { get; set; }
    }
}
