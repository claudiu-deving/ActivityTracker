using Xunit;
using Client.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Models;
using Bogus;

namespace Client.Services.Tests
{
	public class GroupsProviderTests
	{
		private readonly GroupsProvider _sut;

		public GroupsProviderTests()
		{
			_sut = new GroupsProvider();

		}
		[Fact()]
		public void GroupActivitiesTest()
		{
			Faker faker = new Faker();
			Faker<Activity> faker2 = new Faker<Activity>();
			faker2.RuleFor(x => x.Name,(faker,x)=>x.Name = faker.Name.Random.Word());
			faker2.RuleFor(x => x.Duration, (faker, x) => x.Duration = TimeSpan.FromSeconds(faker.Random.Int(0)));
			var actities = faker2.Generate(20);
			_sut.GroupViewModels(actities,)
		}
	}
}