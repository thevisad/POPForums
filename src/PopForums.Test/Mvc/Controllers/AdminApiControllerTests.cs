﻿using Microsoft.AspNetCore.Mvc;
using Moq;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Mvc.Areas.Forums.Controllers;
using PopForums.Services;
using Xunit;

namespace PopForums.Test.Mvc.Controllers
{
	public class AdminApiControllerTests
	{
		private Mock<ISettingsManager> _settingsManager;
		private Mock<ICategoryService> _categoryService;
		private Mock<IForumService> _forumService;

		private AdminApiController GetController()
		{
			_settingsManager = new Mock<ISettingsManager>();
			_categoryService = new Mock<ICategoryService>();
			_forumService = new Mock<IForumService>();
			return new AdminApiController(_settingsManager.Object, _categoryService.Object, _forumService.Object);
		}

		public class SaveForum : AdminApiControllerTests
		{
			[Fact]
			public void CallsCreateIfForumIDIsZero()
			{
				var controller = GetController();
				var forum = new Forum {ForumID = 0, CategoryID = 1, Title = "tt", Description = "dd", IsVisible = true, IsArchived = true, IsQAForum = true, ForumAdapterName = "ff"};

				controller.SaveForum(forum);

				_forumService.Verify(x => x.Create(forum.CategoryID, forum.Title, forum.Description, forum.IsVisible, forum.IsArchived, -1, forum.ForumAdapterName, forum.IsQAForum), Times.Once);
				_forumService.Verify(x => x.Update(It.IsAny<Forum>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
			}

			[Fact]
			public void CallsUpdateIfForumIDIsNotZero()
			{
				var controller = GetController();
				var forum = new Forum { ForumID = 123, CategoryID = 1, Title = "tt", Description = "dd", IsVisible = true, IsArchived = true, IsQAForum = true, ForumAdapterName = "ff" };
				var retrievedForum = new Forum();
				_forumService.Setup(x => x.Get(forum.ForumID)).Returns(retrievedForum);

				controller.SaveForum(forum);

				_forumService.Verify(x => x.Update(retrievedForum, forum.CategoryID, forum.Title, forum.Description, forum.IsVisible, forum.IsArchived, forum.ForumAdapterName, forum.IsQAForum), Times.Once);
				_forumService.Verify(x => x.Create(It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
			}
			[Fact]
			public void ReturnsNotFoundIfForumIsNotReal()
			{
				var controller = GetController();
				_forumService.Setup(x => x.Get(It.IsAny<int>())).Returns((Forum)null);

				var result = controller.SaveForum(new Forum{ForumID = 123});

				_forumService.Verify(x => x.Update(It.IsAny<Forum>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
				_forumService.Verify(x => x.Create(It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
				Assert.IsType<NotFoundResult>(result.Result);
			}
		}
	}
}