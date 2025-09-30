# UFrame - Unity 游戏开发框架

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Unity Version](https://img.shields.io/badge/Unity-2021.3+-green.svg)](https://unity3d.com/)
[![Author](https://img.shields.io/badge/author-zouhunter-orange.svg)](https://github.com/zouhunter)

## 📖 项目简介

UFrame 是一个专为 Unity 游戏开发设计的模块化框架，提供了完整的游戏开发解决方案。框架采用模块化设计，支持按需引入，包含行为树、UI系统、网络通信、资源管理、任务调度等核心功能模块。

## 🏗️ 系统架构

UFrame 采用分层架构设计，主要分为以下几个层次：

```
┌─────────────────────────────────────────────────────────────┐
│                    应用层 (Application)                      │
├─────────────────────────────────────────────────────────────┤
│                    UI层 (BridgeUI)                          │
├─────────────────────────────────────────────────────────────┤
│                  业务逻辑层 (Business Logic)                  │
│  ┌─────────────┬─────────────┬─────────────┬─────────────┐   │
│  │ BehaviourTree│   PureMVC   │   YieldFlow │   Task      │   │
│  └─────────────┴─────────────┴─────────────┴─────────────┘   │
├─────────────────────────────────────────────────────────────┤
│                   网络层 (NetSocket)                         │
├─────────────────────────────────────────────────────────────┤
│                   资源层 (AssetBundles)                      │
├─────────────────────────────────────────────────────────────┤
│                   基础层 (Foundation)                        │
│  ┌─────────────┬─────────────┬─────────────┬─────────────┐   │
│  │   Common    │   Logs      │   Timer     │   Pool      │   │
│  └─────────────┴─────────────┴─────────────┴─────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## 🧩 核心模块

### 🎯 行为树系统 (BehaviourTree)

**功能描述**: 提供完整的 AI 行为树解决方案，支持可视化编辑和运行时执行。

**主要特性**:
- 可视化行为树编辑器
- 支持复合节点、装饰器节点、叶子节点
- 运行时动态执行和状态管理
- 支持条件节点和子条件
- 提供构建工具防止代码裁切

**核心类**:
- `BaseNode`: 行为树节点基类
- `BTree`: 行为树容器
- `TreeInfo`: 树信息管理
- `BehaviourTreeBuildTool`: 构建工具

**使用示例**:
```csharp
using UFrame.BehaviourTree;

// 1. 创建自定义行为节点
[NodePath("AI/攻击敌人")]
public class AttackEnemyNode : ActionNode
{
    public float attackRange = 5f;
    public int damage = 10;
    
    protected override byte OnUpdate()
    {
        var enemy = FindNearestEnemy();
        if (enemy != null && Vector3.Distance(transform.position, enemy.position) <= attackRange)
        {
            enemy.TakeDamage(damage);
            return Status.Success;
        }
        return Status.Failure;
    }
}

// 2. 创建行为树
var bTree = ScriptableObject.CreateInstance<BTree>();
bTree.rootTree = TreeInfo.Create("AI_Tree");

// 3. 设置根节点为选择器
var selector = new SelectorNode();
bTree.rootTree.node = selector;

// 4. 添加子节点
var attackNode = new AttackEnemyNode();
var patrolNode = new PatrolNode();

// 5. 启动行为树
bTree.StartUp();

// 6. 每帧执行
void Update()
{
    byte status = bTree.Tick();
    if (status == Status.Success)
    {
        Debug.Log("AI 任务完成");
    }
}
```

### 🎨 UI 系统 (BridgeUI)

**功能描述**: 现代化的 UI 管理系统，支持面板生命周期、数据绑定和事件系统。

**主要特性**:
- 面板生命周期管理
- 数据绑定和事件系统
- 支持面板组和层级管理
- 自动资源加载和卸载
- 支持 UI 动画和过渡效果

**核心类**:
- `IUIFacade`: UI 外观接口
- `IUIPanel`: 面板接口
- `IPanelVisitor`: 数据访问者模式
- `IPanelGroup`: 面板组管理

**使用示例**:
```csharp
using UFrame.BridgeUI;

// 1. 创建自定义面板
public class MainMenuPanel : MonoBehaviour, IUIPanel
{
    public string PanelName => "MainMenu";
    public bool IsVisible { get; private set; }
    
    public void OnCreate(IPanelVisitor visitor)
    {
        Debug.Log("主菜单面板创建");
        // 初始化UI组件
        SetupUI();
    }
    
    public void OnOpen(object data)
    {
        IsVisible = true;
        gameObject.SetActive(true);
        Debug.Log("主菜单面板打开");
    }
    
    public void OnClose()
    {
        IsVisible = false;
        gameObject.SetActive(false);
        Debug.Log("主菜单面板关闭");
    }
    
    public void OnHide(bool hide)
    {
        IsVisible = !hide;
        gameObject.SetActive(!hide);
    }
    
    public void OnReceive(object data)
    {
        // 处理接收到的数据
        Debug.Log($"接收到数据: {data}");
    }
    
    private void SetupUI()
    {
        // 设置按钮事件
        var startButton = transform.Find("StartButton").GetComponent<Button>();
        startButton.onClick.AddListener(() => {
            UIFacade.Instance.Open("GameUI");
        });
    }
}

// 2. 使用UI系统
public class GameManager : MonoBehaviour
{
    private IUIFacade uiFacade;
    
    void Start()
    {
        // 获取UI外观实例
        uiFacade = SingleUIFacade.Instance;
        
        // 注册面板创建事件
        uiFacade.RegistCreate(OnPanelCreated);
        
        // 打开主菜单
        uiFacade.Open("MainMenu", new { playerName = "Player1" });
    }
    
    private void OnPanelCreated(IUIPanel panel)
    {
        Debug.Log($"面板创建: {panel.PanelName}");
    }
    
    // 切换场景时关闭所有面板
    void OnDestroy()
    {
        uiFacade.Close("MainMenu");
    }
}
```

### 🌐 网络通信 (NetSocket)

**功能描述**: 高性能的网络通信框架，支持 TCP/UDP 协议和消息序列化。

**主要特性**:
- 支持 TCP 和 UDP 协议
- 自动消息序列化和反序列化
- 连接池管理
- 心跳检测和重连机制
- 支持自定义数据包处理器

**核心类**:
- `BuilderBase`: 网络构建器基类
- `PacketHandlerModule`: 数据包处理模块
- `IServiceCollection`: 服务集合接口

**使用示例**:
```csharp
using UFrame.NetSocket;

// 1. 定义数据包
[Packet(0x1001)]
public class LoginPacket : IPacket
{
    public string username;
    public string password;
}

[Packet(0x1002)]
public class LoginResponsePacket : IPacket
{
    public bool success;
    public string message;
    public int playerId;
}

// 2. 创建数据包处理器
public class LoginPacketHandler : IPacketHandler<LoginPacket>
{
    public void Handle(LoginPacket packet, ISession session)
    {
        // 验证用户登录
        bool isValid = ValidateUser(packet.username, packet.password);
        
        var response = new LoginResponsePacket
        {
            success = isValid,
            message = isValid ? "登录成功" : "用户名或密码错误",
            playerId = isValid ? GetPlayerId(packet.username) : 0
        };
        
        // 发送响应
        session.Send(response);
    }
}

// 3. 配置网络服务
public class NetworkManager : MonoBehaviour
{
    private ISession session;
    
    void Start()
    {
        // 构建网络客户端
        var client = new SocketClientBuilder()
            .ConfigureServices(services =>
            {
                services.AddPacketHandler<LoginPacket, LoginPacketHandler>();
                services.AddPacketSerialiser<JsonPacketSerialiser>();
            })
            .Build();
            
        // 连接到服务器
        session = client.ConnectAsync("127.0.0.1", 8080).Result;
        
        // 发送登录请求
        var loginPacket = new LoginPacket
        {
            username = "player1",
            password = "password123"
        };
        session.Send(loginPacket);
    }
    
    void OnDestroy()
    {
        session?.Disconnect();
    }
}
```

### 📦 资源管理 (AssetBundles)

**功能描述**: 完整的资源管理系统，支持 AssetBundle 打包、加载和热更新。

**主要特性**:
- AssetBundle 自动打包
- 资源依赖管理
- 热更新支持
- 资源版本控制
- 内存优化和缓存策略

**使用示例**:
```csharp
using UFrame.AssetBundles;

// 1. 资源加载管理器
public class ResourceManager : MonoBehaviour
{
    private AssetBundleManager bundleManager;
    
    void Start()
    {
        // 初始化资源管理器
        bundleManager = new AssetBundleManager();
        bundleManager.Initialize();
        
        // 加载资源包
        LoadAssetBundle("ui_common");
    }
    
    private async void LoadAssetBundle(string bundleName)
    {
        try
        {
            // 异步加载资源包
            var bundle = await bundleManager.LoadBundleAsync(bundleName);
            
            // 从资源包中加载预制体
            var prefab = await bundle.LoadAssetAsync<GameObject>("MainMenu");
            
            // 实例化预制体
            var instance = Instantiate(prefab);
            Debug.Log($"成功加载资源: {bundleName}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"加载资源失败: {ex.Message}");
        }
    }
    
    // 2. 热更新检查
    public async void CheckForUpdates()
    {
        var updateManager = new HotUpdateManager();
        
        // 检查服务器版本
        var serverVersion = await updateManager.GetServerVersion();
        var localVersion = updateManager.GetLocalVersion();
        
        if (serverVersion > localVersion)
        {
            Debug.Log("发现新版本，开始更新...");
            
            // 下载更新包
            await updateManager.DownloadUpdates(serverVersion);
            
            // 应用更新
            updateManager.ApplyUpdates();
            
            Debug.Log("更新完成，请重启游戏");
        }
    }
}
```

### 🔄 任务流管理 (YieldFlow)

**功能描述**: 基于协程的任务流管理系统，支持任务依赖和并发执行。

**主要特性**:
- 任务依赖管理
- 并发任务执行
- 自动任务回收
- 状态流转控制
- 支持自定义任务节点

**核心类**:
- `FlowManager`: 任务流管理器
- `FlowNode`: 任务节点基类
- `FlowQueue`: 任务队列

**使用示例**:
```csharp
using UFrame.YieldFlow;

// 1. 创建自定义任务节点
public class LoadAssetNode : FlowNode
{
    private string assetPath;
    private GameObject loadedAsset;
    
    public LoadAssetNode(string path)
    {
        name = $"LoadAsset_{path}";
        assetPath = path;
    }
    
    protected override IEnumerator Execute()
    {
        Status = Status.Running;
        
        // 异步加载资源
        var request = Resources.LoadAsync<GameObject>(assetPath);
        yield return request;
        
        if (request.asset != null)
        {
            loadedAsset = request.asset as GameObject;
            Status = Status.Success;
            Debug.Log($"资源加载成功: {assetPath}");
        }
        else
        {
            Status = Status.Failure;
            Debug.LogError($"资源加载失败: {assetPath}");
        }
    }
}

public class InstantiateNode : FlowNode
{
    private GameObject prefab;
    private GameObject instance;
    
    public InstantiateNode(GameObject prefab)
    {
        name = "Instantiate";
        this.prefab = prefab;
    }
    
    protected override IEnumerator Execute()
    {
        Status = Status.Running;
        
        if (prefab != null)
        {
            instance = Instantiate(prefab);
            Status = Status.Success;
            Debug.Log("对象实例化成功");
        }
        else
        {
            Status = Status.Failure;
            Debug.LogError("预制体为空，无法实例化");
        }
        
        yield return null;
    }
}

// 2. 使用任务流管理器
public class GameFlowManager : MonoBehaviour
{
    void Start()
    {
        // 初始化任务流管理器
        var flowManager = new FlowManager(this);
        
        // 创建任务流
        var loadFlow = new FlowBuilder()
            .AddNode(new LoadAssetNode("Prefabs/Player"))
            .AddNode(new InstantiateNode(null)) // 依赖前一个节点的结果
            .Build();
            
        // 启动任务流
        flowManager.StartFlow("LoadPlayer", loadFlow);
        
        // 创建并行任务流
        var parallelFlow = new FlowBuilder()
            .AddParallelNodes(
                new LoadAssetNode("Prefabs/Enemy1"),
                new LoadAssetNode("Prefabs/Enemy2"),
                new LoadAssetNode("Prefabs/Enemy3")
            )
            .Build();
            
        flowManager.StartFlow("LoadEnemies", parallelFlow);
    }
}
```

### 🏛️ 架构模式 (PureMVC)

**功能描述**: 基于 PureMVC 模式的架构框架，提供 Model-View-Controller 分离。

**主要特性**:
- Model-View-Controller 分离
- 观察者模式实现
- 命令模式支持
- 代理模式管理
- 外观模式统一接口

**核心类**:
- `Facade`: 外观模式实现
- `IModel`: 模型接口
- `IView`: 视图接口
- `IController`: 控制器接口

## 🛠️ 工具模块

### 📊 数据配置 (TableCfg)

**功能描述**: 表格数据配置系统，支持 Excel 导入和运行时数据访问。

**主要特性**:
- Excel 表格导入
- 自动代码生成
- 运行时数据访问
- 数据验证和类型检查

**使用示例**:
```csharp
using UFrame.TableCfg;

// 1. 定义配置数据结构
[TableConfig("ItemConfig")]
public class ItemConfigData
{
    public int id;
    public string name;
    public int type;
    public float price;
    public string description;
}

// 2. 使用配置管理器
public class ConfigManager : MonoBehaviour
{
    private TableConfigManager configManager;
    
    void Start()
    {
        // 初始化配置管理器
        configManager = new TableConfigManager();
        configManager.LoadAllConfigs();
        
        // 获取物品配置
        var itemConfig = configManager.GetConfig<ItemConfigData>();
        
        // 根据ID查找物品
        var item = itemConfig.GetById(1001);
        if (item != null)
        {
            Debug.Log($"物品名称: {item.name}, 价格: {item.price}");
        }
        
        // 根据类型筛选物品
        var weapons = itemConfig.GetByType(1); // 武器类型
        foreach (var weapon in weapons)
        {
            Debug.Log($"武器: {weapon.name}");
        }
    }
}

// 3. 在游戏中使用配置
public class ItemManager : MonoBehaviour
{
    public void CreateItem(int itemId)
    {
        var config = TableConfigManager.Instance.GetConfig<ItemConfigData>();
        var itemData = config.GetById(itemId);
        
        if (itemData != null)
        {
            // 创建物品实例
            var item = new GameItem
            {
                id = itemData.id,
                name = itemData.name,
                type = itemData.type,
                price = itemData.price
            };
            
            Debug.Log($"创建物品: {item.name}");
        }
    }
}
```

### 🎵 音频管理 (AudioPlayer)

**功能描述**: 音频播放管理系统，支持背景音乐、音效和 3D 音频。

**主要特性**:
- 多音频源管理
- 3D 音频支持
- 音频淡入淡出
- 音频池管理

**使用示例**:
```csharp
using UFrame.AudioPlayer;

// 1. 音频管理器
public class AudioManager : MonoBehaviour
{
    private AudioPlayerManager audioManager;
    
    void Start()
    {
        // 初始化音频管理器
        audioManager = new AudioPlayerManager();
        audioManager.Initialize();
        
        // 播放背景音乐
        PlayBackgroundMusic();
        
        // 播放音效
        PlaySoundEffect("button_click");
    }
    
    private void PlayBackgroundMusic()
    {
        // 播放背景音乐，支持循环和淡入
        audioManager.PlayMusic("bgm_main", true, 2f); // 2秒淡入
    }
    
    private void PlaySoundEffect(string soundName)
    {
        // 播放音效
        audioManager.PlaySFX(soundName, 1f); // 音量1.0
    }
    
    // 3D 音频示例
    public void Play3DAudio(Vector3 position, string audioClip)
    {
        // 在指定位置播放3D音频
        audioManager.Play3D(audioClip, position, 10f); // 10米范围
    }
    
    // 音频控制
    public void SetMusicVolume(float volume)
    {
        audioManager.SetMusicVolume(volume);
    }
    
    public void SetSFXVolume(float volume)
    {
        audioManager.SetSFXVolume(volume);
    }
    
    public void PauseMusic()
    {
        audioManager.PauseMusic();
    }
    
    public void ResumeMusic()
    {
        audioManager.ResumeMusic();
    }
}

// 2. 音频事件系统
public class AudioEventHandler : MonoBehaviour
{
    void Start()
    {
        // 注册音频事件
        AudioEventManager.Instance.OnMusicFinished += OnMusicFinished;
        AudioEventManager.Instance.OnSFXFinished += OnSFXFinished;
    }
    
    private void OnMusicFinished(string musicName)
    {
        Debug.Log($"背景音乐播放完成: {musicName}");
        
        // 自动播放下一首
        if (musicName == "bgm_main")
        {
            AudioManager.Instance.PlayMusic("bgm_battle", true);
        }
    }
    
    private void OnSFXFinished(string sfxName)
    {
        Debug.Log($"音效播放完成: {sfxName}");
    }
}
```

### 📱 输入系统 (Input)

**功能描述**: 统一的输入管理系统，支持键盘、鼠标、触屏和手柄输入。

**主要特性**:
- 多平台输入支持
- 输入事件系统
- 手势识别
- 输入映射配置

**使用示例**:
```csharp
using UFrame.Input;

// 1. 输入管理器
public class InputManager : MonoBehaviour
{
    private InputSystem inputSystem;
    
    void Start()
    {
        // 初始化输入系统
        inputSystem = new InputSystem();
        inputSystem.Initialize();
        
        // 注册输入事件
        RegisterInputEvents();
    }
    
    private void RegisterInputEvents()
    {
        // 键盘输入
        inputSystem.OnKeyDown += OnKeyDown;
        inputSystem.OnKeyUp += OnKeyUp;
        
        // 鼠标输入
        inputSystem.OnMouseClick += OnMouseClick;
        inputSystem.OnMouseMove += OnMouseMove;
        
        // 触屏输入
        inputSystem.OnTouchStart += OnTouchStart;
        inputSystem.OnTouchEnd += OnTouchEnd;
        
        // 手柄输入
        inputSystem.OnGamepadButton += OnGamepadButton;
    }
    
    private void OnKeyDown(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.Space:
                Debug.Log("跳跃");
                break;
            case KeyCode.Escape:
                Debug.Log("暂停游戏");
                break;
        }
    }
    
    private void OnMouseClick(Vector2 position, int button)
    {
        Debug.Log($"鼠标点击: {position}, 按钮: {button}");
    }
    
    private void OnTouchStart(Vector2 position, int fingerId)
    {
        Debug.Log($"触摸开始: {position}, 手指ID: {fingerId}");
    }
    
    private void OnGamepadButton(GamepadButton button)
    {
        Debug.Log($"手柄按钮: {button}");
    }
}

// 2. 输入映射配置
public class InputMapping : MonoBehaviour
{
    void Start()
    {
        // 配置输入映射
        var inputMapper = InputMapper.Instance;
        
        // 设置动作映射
        inputMapper.MapAction("Jump", KeyCode.Space);
        inputMapper.MapAction("Jump", GamepadButton.A);
        inputMapper.MapAction("Jump", TouchGesture.Tap);
        
        inputMapper.MapAction("Move", KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D);
        inputMapper.MapAction("Move", GamepadAxis.LeftStick);
        
        // 设置轴映射
        inputMapper.MapAxis("Horizontal", KeyCode.A, KeyCode.D);
        inputMapper.MapAxis("Vertical", KeyCode.W, KeyCode.S);
        inputMapper.MapAxis("MouseX", MouseAxis.X);
        inputMapper.MapAxis("MouseY", MouseAxis.Y);
    }
    
    void Update()
    {
        // 检查动作输入
        if (InputMapper.Instance.GetAction("Jump"))
        {
            Debug.Log("跳跃动作触发");
        }
        
        // 获取轴输入
        float horizontal = InputMapper.Instance.GetAxis("Horizontal");
        float vertical = InputMapper.Instance.GetAxis("Vertical");
        
        if (horizontal != 0 || vertical != 0)
        {
            Vector3 movement = new Vector3(horizontal, 0, vertical);
            transform.Translate(movement * Time.deltaTime);
        }
    }
}
```

### 🎬 动画系统 (Tween)

**功能描述**: 高性能的补间动画系统，支持各种缓动函数和动画链。

**主要特性**:
- 丰富的缓动函数
- 动画链和序列
- 路径动画
- 性能优化

**使用示例**:
```csharp
using UFrame.Tween;

// 1. 基础动画
public class TweenExample : MonoBehaviour
{
    void Start()
    {
        // 移动动画
        transform.DOMove(new Vector3(10, 0, 0), 2f)
            .SetEase(Ease.OutBounce)
            .OnComplete(() => Debug.Log("移动完成"));
            
        // 缩放动画
        transform.DOScale(Vector3.one * 2f, 1f)
            .SetEase(Ease.InOutQuad)
            .SetLoops(3, LoopType.Yoyo);
            
        // 旋转动画
        transform.DORotate(new Vector3(0, 360, 0), 2f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear);
    }
}

// 2. 动画序列
public class AnimationSequence : MonoBehaviour
{
    void Start()
    {
        // 创建动画序列
        var sequence = DOTween.Sequence();
        
        // 添加动画到序列
        sequence.Append(transform.DOMoveX(5f, 1f));
        sequence.Append(transform.DOScale(Vector3.one * 1.5f, 0.5f));
        sequence.Append(transform.DORotateZ(180f, 1f));
        
        // 并行执行动画
        sequence.Join(GetComponent<Renderer>().material.DOColor(Color.red, 2f));
        
        // 设置回调
        sequence.OnComplete(() => {
            Debug.Log("动画序列完成");
        });
        
        // 播放序列
        sequence.Play();
    }
}

// 3. UI 动画
public class UIAnimation : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public RectTransform panel;
    
    void Start()
    {
        // 淡入动画
        canvasGroup.DOFade(1f, 0.5f);
        
        // 从下方滑入
        panel.anchoredPosition = new Vector2(0, -Screen.height);
        panel.DOAnchorPosY(0, 0.5f).SetEase(Ease.OutBack);
        
        // 按钮点击动画
        var button = GetComponent<Button>();
        button.onClick.AddListener(() => {
            // 点击缩放效果
            button.transform.DOScale(0.9f, 0.1f)
                .OnComplete(() => {
                    button.transform.DOScale(1f, 0.1f);
                });
        });
    }
    
    public void ClosePanel()
    {
        // 关闭动画
        var sequence = DOTween.Sequence();
        sequence.Append(canvasGroup.DOFade(0f, 0.3f));
        sequence.Join(panel.DOAnchorPosY(-Screen.height, 0.3f));
        sequence.OnComplete(() => {
            gameObject.SetActive(false);
        });
    }
}
```

### 🗄️ 对象池 (Pool)

**功能描述**: 对象池管理系统，减少内存分配和垃圾回收压力。

**主要特性**:
- 自动对象回收
- 预分配策略
- 池大小管理
- 类型安全

**使用示例**:
```csharp
using UFrame.Pool;

// 1. 对象池管理器
public class PoolManager : MonoBehaviour
{
    private ObjectPoolManager poolManager;
    
    void Start()
    {
        // 初始化对象池管理器
        poolManager = new ObjectPoolManager();
        
        // 创建对象池
        CreatePools();
    }
    
    private void CreatePools()
    {
        // 创建子弹对象池
        poolManager.CreatePool<Bullet>("BulletPool", 
            () => Instantiate(bulletPrefab), 
            bullet => bullet.gameObject.SetActive(false),
            bullet => bullet.gameObject.SetActive(true),
            50); // 预分配50个对象
            
        // 创建敌人对象池
        poolManager.CreatePool<Enemy>("EnemyPool",
            () => Instantiate(enemyPrefab),
            enemy => enemy.gameObject.SetActive(false),
            enemy => enemy.gameObject.SetActive(true),
            20);
    }
    
    public void SpawnBullet(Vector3 position, Vector3 direction)
    {
        // 从池中获取子弹
        var bullet = poolManager.Get<Bullet>("BulletPool");
        bullet.transform.position = position;
        bullet.SetDirection(direction);
        bullet.gameObject.SetActive(true);
        
        // 设置自动回收
        StartCoroutine(ReturnBulletAfterDelay(bullet, 5f));
    }
    
    private IEnumerator ReturnBulletAfterDelay(Bullet bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 回收到池中
        poolManager.Return("BulletPool", bullet);
    }
}

// 2. 自定义对象池
public class BulletPool : MonoBehaviour
{
    private Queue<Bullet> bulletPool = new Queue<Bullet>();
    private List<Bullet> activeBullets = new List<Bullet>();
    
    public Bullet GetBullet()
    {
        Bullet bullet;
        
        if (bulletPool.Count > 0)
        {
            bullet = bulletPool.Dequeue();
        }
        else
        {
            bullet = Instantiate(bulletPrefab);
        }
        
        activeBullets.Add(bullet);
        bullet.gameObject.SetActive(true);
        return bullet;
    }
    
    public void ReturnBullet(Bullet bullet)
    {
        if (activeBullets.Contains(bullet))
        {
            activeBullets.Remove(bullet);
            bullet.gameObject.SetActive(false);
            bulletPool.Enqueue(bullet);
        }
    }
    
    // 回收所有活跃的子弹
    public void ReturnAllBullets()
    {
        for (int i = activeBullets.Count - 1; i >= 0; i--)
        {
            ReturnBullet(activeBullets[i]);
        }
    }
}
```

### ⏰ 定时器 (Timer)

**功能描述**: 高精度定时器系统，支持延迟执行和周期性任务。

**主要特性**:
- 高精度计时
- 延迟执行
- 周期性任务
- 时间缩放支持

**使用示例**:
```csharp
using UFrame.Timer;

// 1. 定时器管理器
public class TimerManager : MonoBehaviour
{
    private TimerSystem timerSystem;
    
    void Start()
    {
        // 初始化定时器系统
        timerSystem = new TimerSystem();
        
        // 使用定时器
        UseTimers();
    }
    
    private void UseTimers()
    {
        // 延迟执行
        timerSystem.Delay(2f, () => {
            Debug.Log("2秒后执行");
        });
        
        // 周期性任务
        timerSystem.Repeat(1f, () => {
            Debug.Log("每秒执行一次");
        });
        
        // 带参数的定时器
        timerSystem.Delay(3f, (data) => {
            Debug.Log($"延迟执行，参数: {data}");
        }, "Hello Timer");
        
        // 可取消的定时器
        var timerId = timerSystem.Delay(5f, () => {
            Debug.Log("这个不会执行");
        });
        
        // 取消定时器
        timerSystem.Cancel(timerId);
    }
}

// 2. 游戏中的定时器应用
public class GameTimer : MonoBehaviour
{
    private TimerSystem timerSystem;
    
    void Start()
    {
        timerSystem = new TimerSystem();
        
        // 游戏开始倒计时
        StartCountdown();
        
        // 定期生成敌人
        SpawnEnemiesPeriodically();
        
        // 定期保存游戏
        AutoSaveGame();
    }
    
    private void StartCountdown()
    {
        int countdown = 3;
        
        var countdownTimer = timerSystem.Repeat(1f, () => {
            Debug.Log($"游戏开始倒计时: {countdown}");
            countdown--;
            
            if (countdown <= 0)
            {
                Debug.Log("游戏开始！");
                timerSystem.Cancel(countdownTimer);
            }
        });
    }
    
    private void SpawnEnemiesPeriodically()
    {
        timerSystem.Repeat(2f, () => {
            SpawnEnemy();
        });
    }
    
    private void AutoSaveGame()
    {
        timerSystem.Repeat(30f, () => {
            SaveGame();
        });
    }
    
    private void SpawnEnemy()
    {
        // 生成敌人逻辑
        Debug.Log("生成敌人");
    }
    
    private void SaveGame()
    {
        // 保存游戏逻辑
        Debug.Log("自动保存游戏");
    }
}

// 3. 高级定时器功能
public class AdvancedTimer : MonoBehaviour
{
    private TimerSystem timerSystem;
    
    void Start()
    {
        timerSystem = new TimerSystem();
        
        // 时间缩放
        timerSystem.SetTimeScale(0.5f); // 慢动作
        
        // 帧率限制的定时器
        timerSystem.DelayFrames(60, () => {
            Debug.Log("60帧后执行");
        });
        
        // 条件定时器
        timerSystem.DelayUntil(() => {
            return Input.GetKeyDown(KeyCode.Space);
        }, () => {
            Debug.Log("按下空格键后执行");
        });
    }
}
```

### 📝 日志系统 (Logs)

**功能描述**: 分级日志系统，支持控制台输出和文件记录。

**主要特性**:
- 多级别日志
- 文件输出
- 日志过滤
- 性能监控

**使用示例**:
```csharp
using UFrame.Logs;

// 1. 基础日志使用
public class LogExample : MonoBehaviour
{
    void Start()
    {
        // 初始化日志系统
        LogManager.Initialize();
        
        // 不同级别的日志
        LogManager.Debug("调试信息");
        LogManager.Info("普通信息");
        LogManager.Warning("警告信息");
        LogManager.Error("错误信息");
        LogManager.Fatal("致命错误");
        
        // 带标签的日志
        LogManager.Info("玩家登录", "Player");
        LogManager.Error("网络连接失败", "Network");
        
        // 格式化日志
        LogManager.Info($"玩家 {playerName} 获得 {exp} 经验值");
    }
}

// 2. 高级日志配置
public class AdvancedLogging : MonoBehaviour
{
    void Start()
    {
        // 配置日志系统
        var logConfig = new LogConfig
        {
            EnableConsole = true,
            EnableFile = true,
            LogLevel = LogLevel.Info,
            FilePath = "Logs/game.log",
            MaxFileSize = 10 * 1024 * 1024, // 10MB
            MaxFiles = 5
        };
        
        LogManager.Configure(logConfig);
        
        // 设置日志过滤器
        LogManager.SetFilter("Network", LogLevel.Warning);
        LogManager.SetFilter("UI", LogLevel.Error);
        
        // 性能监控日志
        LogManager.StartPerformanceTimer("GameUpdate");
    }
    
    void Update()
    {
        // 性能监控
        LogManager.LogPerformance("GameUpdate", () => {
            // 游戏更新逻辑
            UpdateGame();
        });
    }
    
    private void UpdateGame()
    {
        // 游戏逻辑
    }
}

// 3. 自定义日志处理器
public class CustomLogHandler : ILogHandler
{
    public void Log(LogLevel level, string message, string tag = null)
    {
        // 自定义日志处理逻辑
        var timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var logMessage = $"[{timestamp}] [{level}] {message}";
        
        if (!string.IsNullOrEmpty(tag))
        {
            logMessage = $"[{tag}] {logMessage}";
        }
        
        // 发送到服务器
        SendToServer(logMessage);
        
        // 写入本地文件
        WriteToFile(logMessage);
    }
    
    private void SendToServer(string message)
    {
        // 发送日志到服务器
    }
    
    private void WriteToFile(string message)
    {
        // 写入本地文件
    }
}

// 4. 游戏中的日志应用
public class GameLogger : MonoBehaviour
{
    void Start()
    {
        // 注册自定义日志处理器
        LogManager.RegisterHandler(new CustomLogHandler());
        
        // 游戏事件日志
        LogManager.Info("游戏启动", "Game");
        LogManager.Info("场景加载完成", "Scene");
    }
    
    public void OnPlayerLogin(string playerName)
    {
        LogManager.Info($"玩家登录: {playerName}", "Player");
    }
    
    public void OnPlayerLogout(string playerName)
    {
        LogManager.Info($"玩家登出: {playerName}", "Player");
    }
    
    public void OnGameError(string error)
    {
        LogManager.Error($"游戏错误: {error}", "Game");
    }
    
    void OnDestroy()
    {
        LogManager.Info("游戏关闭", "Game");
        LogManager.Shutdown();
    }
}
```

## 🚀 快速开始

### 环境要求

- Unity 2022.3 或更高版本
- .NET Standard 2.1
- 支持平台: Windows, macOS, Linux, Android, iOS

### 安装步骤

1. **克隆项目**
```bash
git clone https://github.com/zouhunter/uframe.git
```

2. **导入到 Unity**
- 打开 Unity Hub
- 创建新项目或打开现有项目
- 将 Assets 文件夹复制到项目根目录

3. **配置项目**
- 在 Unity 中打开项目
- 检查 Package Manager 中的依赖包
- 根据需要启用/禁用模块

### 基础使用

1. **初始化框架**
```csharp
using UFrame;

// 初始化核心管理器
var gameManager = new BaseGameManage();
gameManager.Initialize();
```

2. **使用 UI 系统**
```csharp
using UFrame.BridgeUI;

// 获取 UI 外观
var uiFacade = SingleUIFacade.Instance;

// 打开主菜单
uiFacade.Open("MainMenu");
```

3. **创建行为树**
```csharp
using UFrame.BehaviourTree;

// 创建行为树实例
var bTree = ScriptableObject.CreateInstance<BTree>();

// 设置根节点
bTree.rootTree = TreeInfo.Create();
bTree.rootTree.node = new MyActionNode();

// 启动行为树
bTree.StartUp();
```

## 📚 详细文档

### 模块文档

- [BehaviourTree 行为树系统](./Assets/BehaviourTree/README.md)
- [BridgeUI UI 系统](./Assets/BridgeUI/README.md)
- [NetSocket 网络系统](./Assets/NetSocket/README.md)
- [AssetBundles 资源管理](./Assets/AssetBundles/README.md)
- [YieldFlow 任务流管理](./Assets/YieldFlow/README.md)

### API 文档

详细的 API 文档请参考各模块的代码注释和示例。

## 🎯 最佳实践

### 1. 模块化开发
- 按功能模块组织代码
- 使用依赖注入管理模块间依赖
- 保持模块间的松耦合

### 2. 性能优化
- 使用对象池减少内存分配
- 合理使用协程和异步操作
- 优化资源加载和卸载

### 3. 代码规范
- 遵循 C# 编码规范
- 使用有意义的命名
- 添加必要的注释和文档

## 🤝 贡献指南

我们欢迎社区贡献！请遵循以下步骤：

1. Fork 项目
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 创建 Pull Request

### 贡献规范

- 代码风格保持一致
- 添加必要的测试
- 更新相关文档
- 遵循语义化版本控制

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 👨‍💻 作者

**zouhunter**
- Email: zouhunter@126.com
- GitHub: [@zouhunter](https://github.com/zouhunter)

## 🙏 致谢

感谢所有为 UFrame 框架做出贡献的开发者和社区成员。

## 📞 支持与反馈

如果您在使用过程中遇到问题或有建议，请：

- 提交 [Issue](https://github.com/zouhunter/uframe/issues)
- 发送邮件至 zouhunter@126.com
- 参与 [Discussions](https://github.com/zouhunter/uframe/discussions)

---

**UFrame** - 让 Unity 游戏开发更简单、更高效！