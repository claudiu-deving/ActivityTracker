using BiTLuZ.InfraLib;
using Client.Models;
using Client.Services;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace Client.ViewModels.Tests;

public class TestPathsProvider : IAppPathsProvider,IPathsProvider
{
	public string GetAppFolder()
	{
		return "TestData";
	}

	public string GetGroupsJsonPath()
	{
		return @"TestData\groups.json";
	}
}

public class MainViewModelTests
{
	private readonly MainViewModel _sut;

	public MainViewModelTests()
	{
		var pathsProvider = new TestPathsProvider();
		var appLogger = new AppLogger(pathsProvider);
		var activityService = new ActivityService(pathsProvider);
		var activityGroupService = new ActivityGroupService(pathsProvider, appLogger, activityService);
		_sut = new MainViewModel(activityGroupService,activityService);
	}

	[Fact(DisplayName = "Constructor")]
	public void MainViewModelTest()
	{
		// Arrange
		// Act
		// Assert
		Assert.NotNull(_sut.Activities);
		Assert.NotNull(_sut.ActivityGroups);
		Assert.NotNull(_sut.AvailableColors);
	}

	[Fact(DisplayName = "Initialized correctly")]
	public async Task InitializeTest()
	{
		// Arrange
		// Act
		var result = await _sut.Initialize();
		// Assert
		Assert.True(result.IsSuccess,result.Message);
		Assert.NotNull(_sut.Activities);
		Assert.NotNull(_sut.ActivityGroups);
		Assert.True(_sut.ActivityGroups.Count > 0,"The activity groups must contain at least the Ungrouped group");

	}


	[Fact(DisplayName = "AddGroupCommand adds new activity group to ActivityGroups")]
	public async Task AddActivityCommandTest()
	{
		await _sut.Initialize();

		// Arrange
		// Act
		_sut.AddGroupCommand.Execute(null);
		// Assert
		Assert.True(_sut.ActivityGroups.Count>1);
		Assert.Empty(_sut.ActivityGroups[^1].Activities);
		Assert.Equal("New Group", _sut.ActivityGroups[^1].Name);
	}

	[Fact(DisplayName = "AddPatternCommand adds a new pattern if the condition is met")]
	public async Task AddPatternCommandTest()
	{

		// Arrange
		await _sut.Initialize();
		_sut.SelectedGroup = _sut.ActivityGroups[0];
		_sut.NewPatternInput = "New Pattern";
		// Act
		_sut.AddPatternCommand.Execute(null);
		// Assert
		Assert.True(_sut.SelectedGroup.Patterns.Count>0);
		Assert.Empty(_sut.SelectedGroup.Patterns[^1].Activities);
		Assert.Equal("New Pattern", _sut.SelectedGroup.Patterns[^1].Sentence);
	}

	[Fact(DisplayName = "AddPatternCommand doesn't add a new pattern if the input is empty")]
	public async Task AddPatternCommandTest2()
	{
		// Arrange
		await _sut.Initialize();
		_sut.SelectedGroup = _sut.ActivityGroups[0];
		_sut.NewPatternInput = "";
		// Act
		_sut.AddPatternCommand.Execute(null);
		// Assert
		Assert.Empty(_sut.SelectedGroup.Patterns.Where(x=>string.IsNullOrEmpty(x.Sentence)));
	}

	[Fact(DisplayName = "AddPatternCommand doesn't add a new pattern if the selected group is null")]
	public async Task AddPatternCommandTest3()
	{
		// Arrange
		await _sut.Initialize();
		_sut.SelectedGroup = null;
		_sut.NewPatternInput = "New Pattern";
		// Act
		_sut.AddPatternCommand.Execute(null);
		// Assert
		Assert.Empty(_sut.ActivityGroups[0].Patterns);
	}

	[Fact(DisplayName = "RemovePatternCommand removes the pattern from the selected group")]
	public async Task RemovePatternCommandTest()
	{
		// Arrange
		await _sut.Initialize();
		string patternName = "New Pattern";
		_sut.SelectedGroup = _sut.ActivityGroups[0];
		_sut.NewPatternInput = patternName;
		_sut.AddPatternCommand.Execute(null);
		Assert.True(_sut.SelectedGroup.Patterns.Count>0);
		// Act
		_sut.RemovePatternCommand.Execute(patternName);
		// Assert
		Assert.Empty(_sut.SelectedGroup.Patterns);
	}
}