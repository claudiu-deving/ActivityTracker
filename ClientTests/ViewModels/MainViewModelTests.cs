using Client.Models;
using Client.Services;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace Client.ViewModels.Tests;

public class MainViewModelTests
{
	private readonly Mock<IActivityGroupService> _mockedActivityGroupsService;
	private readonly Mock<IInitializable> _mockedActivityGroupsServiceIni;
	private readonly Mock<IActivityService> _mochedActivityService;
	private readonly MainViewModel _sut;

	public MainViewModelTests()
	{
		 _mockedActivityGroupsService = new Mock<IActivityGroupService>();
		_mockedActivityGroupsServiceIni = _mockedActivityGroupsService.As<IInitializable>();
		_mochedActivityService = new Mock<IActivityService>();


		_sut = new MainViewModel(_mockedActivityGroupsService.Object, _mochedActivityService.Object);
	}

	[Fact(DisplayName = "Activities, groups and colors are populate after constructing")]
	public void MainViewModelTest()
	{
		// Arrange
		// Act
		// Assert
		Assert.NotNull(_sut.Activities);
		Assert.NotNull(_sut.ActivityGroups);
		Assert.NotNull(_sut.AvailableColors);
	}

	[Fact(DisplayName = "Initialize method calls the Initialize method of the ActivityGroupService")]
	public async Task InitializeTest()
	{
		// Arrange
		var activityGroups = new List<ActivityGroup>()
		{
			new()
			{
				Name ="New Group"
			}
		};
		_mockedActivityGroupsService.Setup(x => x.GetActivityGroups()).Returns(ServiceResponse<IEnumerable<ActivityGroup>>.Success(activityGroups));

		_mockedActivityGroupsServiceIni.Setup(x => x.Initialize()).ReturnsAsync(true);
		_mochedActivityService.Setup(x => x.GetActivities()).Returns(ServiceResponse<List<Activity>>.Success(new List<Activity>()));
		// Act
		await _sut.Initialize();
		// Assert
		_mockedActivityGroupsServiceIni.Verify(x =>x.Initialize(), Times.Once);
	}


	[Fact(DisplayName = "AddGroupCommand adds new activity group to ActivityGroups")]
	public async Task AddActivityCommandTest()
	{
		await _sut.Initialize();
		// Arrange
		var activityGroups = new List<ActivityGroup>()
		{
			new()
			{
				Name ="New Group"
			}
		};
		_mockedActivityGroupsService.Setup(x => x.GetActivityGroups()).Returns( ServiceResponse<IEnumerable<ActivityGroup>>.Success(activityGroups));
		// Act
		_sut.AddGroupCommand.Execute(null);
		// Assert
		Assert.True(_sut.ActivityGroups.Count>1);
		Assert.Empty(_sut.ActivityGroups[^1].Activities);
		Assert.Equal("New Group", _sut.ActivityGroups[^1].Name);
	}

	[Fact(DisplayName = "AddPatternCommand adds a new pattern if the condition is met")]
	public void AddPatternCommandTest()
	{
		// Arrange
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
	public void AddPatternCommandTest2()
	{
		// Arrange
		_sut.SelectedGroup = _sut.ActivityGroups[0];
		_sut.NewPatternInput = "";
		// Act
		_sut.AddPatternCommand.Execute(null);
		// Assert
		Assert.Empty(_sut.SelectedGroup.Patterns);
	}

	[Fact(DisplayName = "AddPatternCommand doesn't add a new pattern if the selected group is null")]
	public void AddPatternCommandTest3()
	{
		// Arrange
		_sut.SelectedGroup = null;
		_sut.NewPatternInput = "New Pattern";
		// Act
		_sut.AddPatternCommand.Execute(null);
		// Assert
		Assert.Empty(_sut.ActivityGroups[0].Patterns);
	}

	[Fact(DisplayName = "RemovePatternCommand removes the pattern from the selected group")]
	public void RemovePatternCommandTest()
	{
		// Arrange
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