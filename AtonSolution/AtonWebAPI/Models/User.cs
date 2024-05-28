using AtonWebAPI.Models.Validations;
using System.ComponentModel.DataAnnotations;

namespace AtonWebAPI.Models
{
	public class User
	{
		[Key]
		public Guid Guid { get; set; } = Guid.NewGuid();

		public required string Login { get => _login; set => _login = value; }
		public required string Password { get => _password; set => _password = value; }
		public required string Name { get => _name; set => _name = value; }
		public int Gender { get => _gender; set => _gender = value; }
		public DateTime? Birthday { get => _birthday; set => _birthday = value; }
		public bool Admin { get => _admin; set => _admin = value; }
		
		public required DateTime CreatedOn { get => _createdOn; set => _createdOn = value; }
		public required string CreatedBy { get => _createdBy; set => _createdBy = value; }
		
		public DateTime? ModifiedOn { get => _modifiedOn; set => _modifiedOn = value; }
		public string? ModifiedBy { get => _modifiedBy; set => _modifiedBy = value; }
		
		public DateTime? RevokedOn { get => _revokedOn; set => _revokedOn = value; }
		public string? RevokedBy { get => _revokedBy; set => _revokedBy = value; }



		private string _login = null!;
		private string _password = null!;
		private string _name = null!;
		private int _gender = 2;
		private DateTime? _birthday;
		private bool _admin = false;

		private DateTime _createdOn;
		private string _createdBy = null!;

		private DateTime? _modifiedOn;
		private string? _modifiedBy;

		private DateTime? _revokedOn;
		private string? _revokedBy;
	}
}
