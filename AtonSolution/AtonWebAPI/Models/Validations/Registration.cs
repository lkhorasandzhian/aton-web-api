using System.ComponentModel.DataAnnotations;

namespace AtonWebAPI.Models.Validations
{
	public class Registration
	{
		public User CreateUserByGivenData(string creatorLogin) =>
			new()
			{
				Login = Login,
				Password = Password,
				Name = Name,
				Gender = Gender,
				Birthday = Birthday,
				Admin = Admin,
				CreatedOn = DateTime.Now,
				CreatedBy = creatorLogin
			};

		[Required(ErrorMessage = "Login is required")]
		[RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Login must contain only Latin letters and numbers")]
		public required string Login { get; set; }

		[Required(ErrorMessage = "Password is required")]
		[DataType(DataType.Password)]
		[RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Password must contain only Latin letters and numbers")]
		public required string Password { get; set; }

		[Required(ErrorMessage = "Name is required")]
		[RegularExpression("^[a-zA-Zа-яА-Я]*$", ErrorMessage = "Name must contain only Latin letters and Russian letters")]
		public required string Name { get; set; }

		[Required(ErrorMessage = "Gender is required")]
		[Range(0, 2, ErrorMessage = "Gender value must be in range [0;2] (0 - female, 1 - male, 2 - undefined)")]
		public required int Gender { get; set; } = 2;

		[DataType(DataType.Date, ErrorMessage = "Invalid date format")]
		[DateNotInFuture]
		public DateTime? Birthday { get; set; }

		public bool Admin { get; set; } = false;
	}
}
