/// <summary>
/// シーン名をEnumで管理するクラス
/// </summary>
public enum SceneName
{
	BaseInit,
	One,
	MoveFloor,
	Tower,
	Enemy,
	Warp,
	Elevator,
	NoFloor,
	UnHit,
	Conveyor,
	Moves,
	Enemies,
	OnOff,
	Fork,
	Flip,
	LongJump,
	Mountain,
	Rodeo,
	Red,
	Snake,
	Line,
	Title,
}

/// <summary>
/// タグ名を文字列で管理するクラス
/// </summary>
public static class TagName
{
	public const string Untagged = "Untagged";
	public const string Respawn = "Respawn";
	public const string Finish = "Finish";
	public const string EditorOnly = "EditorOnly";
	public const string MainCamera = "MainCamera";
	public const string Player = "Player";
	public const string GameController = "GameController";
	public const string Damage = "Damage";
	public const string Ground = "Ground";
	public const string Belt = "Belt";
	public const string Item = "Item";
	public const string Enemy = "Enemy";
	public const string Goal = "Goal";
	public const string RespawnPoint = "RespawnPoint";
	public const string Portal = "Portal";
}

/// <summary>
/// レイヤー名を定数で管理するクラス
/// </summary>
public static class LayerName
{
	public const int Default = 0;
	public const int TransparentFX = 1;
	public const int IgnoreRaycast = 2;
	public const int Camera = 3;
	public const int Water = 4;
	public const int UI = 5;
	public const int Hit = 6;
	public const int TrackingHit = 7;
	public const int UnHit = 8;
	public const int DefaultMask = 1;
	public const int TransparentFXMask = 2;
	public const int IgnoreRaycastMask = 4;
	public const int CameraMask = 8;
	public const int WaterMask = 16;
	public const int UIMask = 32;
	public const int HitMask = 64;
	public const int TrackingHitMask = 128;
	public const int UnHitMask = 256;
}

