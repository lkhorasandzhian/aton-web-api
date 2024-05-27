using System.ComponentModel.DataAnnotations;

namespace AtonWebAPI.Models
{
	public class User
	{
		[Key]
		public Guid Guid { get; set; } = Guid.NewGuid();

		[Required(ErrorMessage = "Login is required")]
		[RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Login must contain only Latin letters and numbers")]
		public required string Login { get; set; }

		[Required(ErrorMessage = "Password is required")]
		[RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Password must contain only Latin letters and numbers")]
		public required string Password { get; set; }

		[Required(ErrorMessage = "Name is required")]
		[RegularExpression("^[a-zA-Zа-яА-Я]*$", ErrorMessage = "Name must contain only Latin letters and Russian letters")]
		public required string Name { get; set; }

		[Required(ErrorMessage = "Gender is required")]
		[Range(0, 2, ErrorMessage = "Gender value must be in range [0;2] (0 - female, 1 - male, 2 - undefined)")]
		public int Gender { get; set; } = 2;

		[Required]
		public DateTime? Birthday { get; set; }

		public bool Admin { get; set; }

		private DateTime _createdOn;
		private string _createdBy = string.Empty;

		private DateTime _modifiedOn;
		private string _modifiedBy = string.Empty;

		private DateTime _revokedOn;
		private string _revokedBy = string.Empty;
	}
}
