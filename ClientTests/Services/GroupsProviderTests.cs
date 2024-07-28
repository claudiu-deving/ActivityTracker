using Xunit;
using Client.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Models;
using Bogus;
using Assert = Xunit.Assert;

namespace Client.Services.Tests
{
	public class GroupsProviderTests
	{
		[Fact()]
		public void ReturnsThreeLevelOneGroups_ThreeLevelOneGroupDefinitions()
		{
			var activities = new List<Activity>
			{
				new() {
					Name = "Window 1",
					TotalDuration = TimeSpan.FromSeconds(1),
				},
				new() {
					Name = "Window 2",
					TotalDuration = TimeSpan.FromSeconds(4),
				},
				new() {
					Name = "Window 1 - Project 1",
					TotalDuration = TimeSpan.FromSeconds(4),
				},
				new() {
					Name = "Window 1 - Project 2",
					TotalDuration = TimeSpan.FromSeconds(4),
				},
				new() {
					Name = "Window 3 - Project 2",
					TotalDuration = TimeSpan.FromSeconds(4),
				}
			};
			var groupDefinitions = new List<GroupDefinition>
			{
				new()
				{
					Name ="Window 1",
					Pattern ="Window 1"
				},
				new()
				{
					Name ="Window 2",
					Pattern ="Window 2"
				},
				new()
				{
					Name ="Window 3",
					Pattern ="Window 3"
				}
			};
			var result =	GroupsProvider.GroupViewModels(activities, groupDefinitions);
			Assert.NotNull(result);
			Assert.True(result[0].Activities.Any());
			Assert.True(result[1].Activities.Any());
			Assert.True(result[2].Activities.Any());
			Assert.Equal("Window 1",result[0].Activities[0].Name);
		}

		[Fact()]
		public void GroupsAsExpectedLevelTwoGroupNested()
		{
			var activities = new List<Activity>
			{
				new() {
					Name = "Window 1",
					TotalDuration = TimeSpan.FromSeconds(1),
				},
				new() {
					Name = "Window 1 - Project 1",
					TotalDuration = TimeSpan.FromSeconds(4),
				},
				new() {
					Name = "Window 1 - Project 2",
					TotalDuration = TimeSpan.FromSeconds(4),
				}
			};
			var groupDefinitions = new List<GroupDefinition>
			{
				new()
				{
					Name ="Window 1",
					Pattern ="Window 1",
					GroupDefinitions =
					[
						new()
						{
							Name ="Projects",
							Pattern = "Window 1 - Project"
						}
					]
				}
			};
			var result = GroupsProvider.GroupViewModels(activities, groupDefinitions);
			Assert.NotNull(result);
			Assert.True(result[0].Activities.Any());
			Assert.Equal("Window 1", result[0].Activities[0].Name);
			Assert.Equal("Projects", result[0].Groups[0].Name);
			Assert.Equal(2, result[0].Groups[0].Activities.Count);
		}

		[Fact()]
		public void ReturnAsUngrouped_NoGroupDefinitionsPresent()
		{
			var activities = new List<Activity>
			{
				new() {
					Name = "Window 1",
					TotalDuration = TimeSpan.FromSeconds(1),
				},
				new() {
					Name = "Window 2",
					TotalDuration = TimeSpan.FromSeconds(4),
				},
				new() {
					Name = "Window 1 - Project 1",
					TotalDuration = TimeSpan.FromSeconds(4),
				},
				new() {
					Name = "Window 1 - Project 2",
					TotalDuration = TimeSpan.FromSeconds(4),
				},
				new() {
					Name = "Window 3 - Project 2",
					TotalDuration = TimeSpan.FromSeconds(4),
				}
			};
			
			var result = GroupsProvider.GroupViewModels(activities, []);
			Assert.NotNull(result);
			Assert.True(result[0].Activities.Any());
			Assert.Equal("Ungrouped", result[0].Name);
		}

		[Fact()]
		public void ReturnAsExpected_NonExhaustiveGroupDefinitions()
		{
			var activities = new List<Activity>
			{
				new() {
					Name = "Window 1",
					TotalDuration = TimeSpan.FromSeconds(1),
				},
				new() {
					Name = "Window 2",
					TotalDuration = TimeSpan.FromSeconds(4),
				},
				new() {
					Name = "Window 1 - Project 1",
					TotalDuration = TimeSpan.FromSeconds(4),
				},
				new() {
					Name = "Window 1 - Project 2",
					TotalDuration = TimeSpan.FromSeconds(4),
				},
				new() {
					Name = "Window 3 - Project 2",
					TotalDuration = TimeSpan.FromSeconds(4),
				}
			};
			var groupDefinitions = new List<GroupDefinition>
			{
				new()
				{
					Name ="Window 1",
					Pattern ="Window 1"
				}
			};
			var result = GroupsProvider.GroupViewModels(activities, groupDefinitions);
			Assert.NotNull(result);
			Assert.True(result[0].Activities.Any());
			Assert.True(result[1].Activities.Any());
			Assert.Equal("Window 1", result[0].Name);
			Assert.Equal("Ungrouped", result[1].Name);
		}
	}
}