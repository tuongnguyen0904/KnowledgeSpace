using KnowledgeSpace.BackendServer.Controllers;
using KnowledgeSpace.ViewModels;
using KnowledgeSpace.ViewModels.Systems;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MockQueryable.Moq;


namespace KnowledgeSpace.BackendServer.Test.Controllers;

public class RolesControllerTest
{
    private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
    private List<IdentityRole> _roleSources = new List<IdentityRole>(){
        new IdentityRole("test1"),
        new IdentityRole("test2"),
        new IdentityRole("test3"),
        new IdentityRole("test4")
    };
    public RolesControllerTest()
    {
        var roleStore = new Mock<IRoleStore<IdentityRole>>();
        _mockRoleManager = new Mock<RoleManager<IdentityRole>>(roleStore.Object, null, null, null, null);
    }
    
    [Fact]
    public void ShouldCreateInstance_NotNull_Ok()
    {
        var roleController = new RolesController(_mockRoleManager.Object);
        Assert.NotNull(roleController);
    }
    
    [Fact]
    public async Task PostRole_ValidInput_Success()
    {
        _mockRoleManager.Setup(r => r.CreateAsync(It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success);
        var roleController = new RolesController(_mockRoleManager.Object);
        var result = await roleController.PostRole(new RoleCreateRequest()
        {
            Id = "test",
            Name = "test"
        });
        
        Assert.NotNull(result);
        Assert.IsType<CreatedAtActionResult>(result);
    }
    
    [Fact]
    public async Task PostRole_ValidInput_Failed()
    {
        _mockRoleManager.Setup(r => r.CreateAsync(It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Failed(new IdentityError()));
        var roleController = new RolesController(_mockRoleManager.Object);
        var result = await roleController.PostRole(new RoleCreateRequest()
        {
            Id = "test",
            Name = "test"
        });
        
        Assert.NotNull(result);
        Assert.IsType<BadRequestObjectResult>(result);
    }
    
    [Fact]
    public async Task GetRoles_HasData_ReturnSuccess()
    {
        _mockRoleManager.Setup(x => x.Roles)
            .Returns(_roleSources.AsQueryable().BuildMock().Object);
        var rolesController = new RolesController(_mockRoleManager.Object);
        var result = await rolesController.GetRoles();
        var okResult = result as OkObjectResult;
        var roleVms = okResult.Value as IEnumerable<RoleVm>;
        Assert.True(roleVms.Count() > 0);
    }
    
    [Fact]
    public async Task GetRole_ThrowException_Failed()
    {
        _mockRoleManager.Setup(x => x.Roles).Throws<Exception>();
        var roleController = new RolesController(_mockRoleManager.Object);

        await Assert.ThrowsAnyAsync<Exception>( async () => await roleController.GetRoles());
    }
    
    
    [Fact]
    public async Task GetRolesPaging_NoFilter_ReturnSuccess()
    {
        _mockRoleManager.Setup(x => x.Roles)
            .Returns(_roleSources.AsQueryable().BuildMock().Object);

        var rolesController = new RolesController(_mockRoleManager.Object);
        var result = await rolesController.GetAllRolesPaging(null, 1, 2);
        var okResult = result as OkObjectResult;
        var roleVms = okResult.Value as Pagination<RoleVm>;
        Assert.Equal(4, roleVms.TotalRecords);
        Assert.Equal(2, roleVms.Items.Count);
    }

    [Fact]
    public async Task GetRolesPaging_HasFilter_ReturnSuccess()
    {
        _mockRoleManager.Setup(x => x.Roles)
            .Returns(_roleSources.AsQueryable().BuildMock().Object);

        var rolesController = new RolesController(_mockRoleManager.Object);
        var result = await rolesController.GetAllRolesPaging("test3", 1, 2);
        var okResult = result as OkObjectResult;
        var roleVms = okResult.Value as Pagination<RoleVm>;
        Assert.Equal(1, roleVms.TotalRecords);
        Assert.Single(roleVms.Items);
    }
}