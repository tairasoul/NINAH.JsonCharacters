using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;
using _Code.Characters;
using _Code.Infrastructure.Sound;
using _Code.Menues.HUD.Animations;
using _Code.Rooms;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using DG.Tweening;
using Newtonsoft.Json;
using tairasoul.ninah.characterlib;
using tairasoul.ninah.characterlib.utils;
using UnityEngine;
using UnityEngine.Networking;

namespace tairasoul.ninah.jsoncharacters;

struct JsonVec3 {
  public float x;
  public float y;
  public float z;
}

struct JsonVec2 {
  public float x;
  public float y;
}

struct JsonSprite {
  public string path;
  public JsonVec2? offset;
  public JsonVec2? size;
  public JsonVec2? pivot;
  public float? pixelsPerUnit;
}

struct JsonEyeAnimation {
  public JsonSprite iris;
  public JsonSprite white;
  public string ease;
  public float distanceMultiplier;
  public float minMoveDuration;
  public float maxMoveDuration;
}

struct JsonAnimation {
  public string? cyclingType;
  public JsonSprite[] frames;
  public int? framesPerSecond;
}

struct JsonRoomState {
  public bool? cannotInteract;
  public JsonSprite sprite;
  public JsonVec2 scaleShift;
  public JsonVec2 posShift;
}

struct JsonEmotion {
  public JsonSprite? sprite;
  public JsonAnimation? animation;
}

struct JsonDialogue {
  public string path;
  public string[]? merges;
  public string? locale;
}

struct JsonTalkRedirect {
  public int talkNum;
  public string redirected;
}

struct CharacterJson {
  public string name;
  public string entranceRedirect;
  public JsonDialogue[] dialog;
  public Dictionary<string, JsonEmotion> emotions;
  public string? room;
  public string? roomPosition;
  public string? knockingSound;
  public string? entranceTheme;
  public JsonTalkRedirect[]? talkRedirects;
  public Dictionary<string, string>? emotionRedirects;
  public Dictionary<string, JsonRoomState>? roomStates;
  public string? startingRoomState;
  public JsonSprite? humanTeethSprite;
  public JsonSprite? visitorTeethSprite;
  public JsonEyeAnimation? humanEyeAnimation;
  public JsonEyeAnimation? visitorEyeAnimation;
  public JsonAnimation? humanEarAnimation;
  public JsonAnimation? visitorEarAnimation;
  public JsonAnimation? humanArmpitAnimation;
  public JsonAnimation? visitorArmpitAnimation;
  public JsonSprite? humanHandSprite;
  public JsonSprite? visitorHandSprite;
  public bool? isVisitor;
  public string? nightRoomSoundPath;
  public string? corpseMaskPath;
  public float? femaRatingBonus;
  public int? minimumCompletionsToAppear;
  public JsonVec2? gachaPosBias;
  public JsonVec2? gachaScaleBias;
  public JsonVec2? dialogPosition;
  public float? dialogScale;
  public int? dialogCount;
  public int[]? canAppearDays;
  public bool? isRandom;
  public float? entranceScale;
}

[BepInDependency("tairasoul.ninah.characterlib")]
[BepInPlugin("tairasoul.ninah.jsoncharacters", "JsonCharacters", "1.0.0")]
class Plugin : BasePlugin {
  private Type JsonConvertType;
  private Dictionary<string, Texture2D> existingTextures = [];
  public override void Load() {
    AssemblyLoadContext loadContext = new("NewtonsoftLoadContext", true);
    byte[] newtonsoftBytes = AssemblyUtils.GetResourceBytes("Newtonsoft");
    using MemoryStream ms = new(newtonsoftBytes);
    Assembly contextNewtonsoft = loadContext.LoadFromStream(ms);
    JsonConvertType = contextNewtonsoft.GetType("Newtonsoft.Json.JsonConvert");
    string folder = Path.Join(Paths.PluginPath, "Characters");
    if (!Directory.Exists(folder))
      Directory.CreateDirectory(folder);
    foreach (string dir in Directory.EnumerateDirectories(folder)) {
      string jsonFile = Path.Join(dir, "character.json");
      if (File.Exists(jsonFile)) {
        LoadJsonFile(jsonFile);
      }
      else {
        Log.LogError($"Expected character.json in directory Characters/{new DirectoryInfo(dir).Name}.");
      }
    }
    loadContext.Unload();
  }

  void LoadJsonFile(string file) {
    string text = File.ReadAllText(file);
    CharacterJson cjson = (CharacterJson)JsonConvertType.GetMethods(BindingFlags.Static | BindingFlags.Public).First((v) => v.IsGenericMethod && v.Name == "DeserializeObject" && v.GetParameters().First().ParameterType == typeof(string)).MakeGenericMethod(typeof(CharacterJson)).Invoke(null, [text]);
    CustomCharacterBuilder builder = new(cjson.name);
    string basePath = Path.GetDirectoryName(file);
    if (cjson.emotionRedirects != null)
      builder.SetEmotionRedirects(cjson.emotionRedirects);
    if (cjson.talkRedirects != null) {
      foreach (JsonTalkRedirect redirect in cjson.talkRedirects)
        builder.SetTalkRedirect(redirect.talkNum, redirect.redirected);
    }
    if (cjson.minimumCompletionsToAppear.HasValue)
      builder.SetMinimumCompletionsToAppear(cjson.minimumCompletionsToAppear.Value);
    if (cjson.entranceScale.HasValue)
      builder.SetEntranceScale(cjson.entranceScale.Value);
    if (cjson.dialogPosition.HasValue)
      builder.SetDialogPosition(cjson.dialogPosition.Value.x, cjson.dialogPosition.Value.y);
    if (cjson.dialogScale.HasValue)
      builder.SetDialogScale(cjson.dialogScale.Value);
    if (cjson.canAppearDays != null)
      builder.SetDaysCanAppear(cjson.canAppearDays);
    builder.SetEntranceRedirect(cjson.entranceRedirect);
    Load(basePath, builder, cjson);
  }

  void Load(string basePath, CustomCharacterBuilder builder, CharacterJson cjson) {
    List<string> errors = [];
    bool HadError = false;
    foreach (JsonDialogue dialogue in cjson.dialog) {
      if (!ProcessDialog(basePath, cjson.name, dialogue, errors))
        HadError = true;
    }
    List<ACharacterSpriteByEmotion> byEmotions = [];
    foreach (KeyValuePair<string, JsonEmotion> emotion in cjson.emotions) {
      if (Enum.TryParse(emotion.Key, true, out EDialogEmotionState state)) {
        JsonEmotion json = emotion.Value;
        if (json.animation.HasValue) {
          if (processMiscAnimation(basePath, json.animation.Value, errors, $"for emotion {emotion.Key} on character {cjson.name}", out var anim)) {
            byEmotions.Add(new CharacterAnimatedSpriteByEmotion()
            {
              _animationData = new(anim.Value.Item1, (Sprite[])[.. anim.Value.Item2], anim.Value.Item3),
              _emotion = state
            });
          }
          else {
            HadError = true;
          }
        }
        else {
          if (!json.sprite.HasValue) {
            HadError = true;
            errors.Add($"Expected animation or sprite for emotion {emotion.Key}!");
          }
          else
          {
            if (processSprite(basePath, json.sprite.Value, out Sprite sprite))
            {
              byEmotions.Add(new CharacterBaseSpriteByEmotionData()
              {
                _emotion = state,
                _sprite = sprite
              });
            }
            else
            {
              HadError = true;
              errors.Add($"Got non-existent file {json.sprite.Value.path} for emotion {emotion.Key} on character {cjson.name}");
            }
          }
        }
      }
      else {
        HadError = true;
        errors.Add($"Got invalid emotion {emotion.Key} for character {cjson.name}");
        JsonEmotion json = emotion.Value;
        if (json.animation.HasValue) {
          if (!processMiscAnimation(basePath, json.animation.Value, errors, $"for emotion {emotion.Key} on character {cjson.name}", out var anim)) {
            HadError = true;
          }
        }
        else {
          if (!json.sprite.HasValue) {
            HadError = true;
            errors.Add($"Expected animation or sprite for emotion {emotion.Key}!");
          }
          else
            if (!processSprite(basePath, json.sprite.Value, out Sprite sprite)) {
              HadError = true;
              errors.Add($"Got non-existent file {json.sprite.Value.path} for emotion {emotion.Key} on character {cjson.name}");
            }
        }
      }
    }
    builder.SetEmotions(byEmotions);
    if (cjson.isRandom.HasValue)
      builder.SetRandomlyGenerated(cjson.isRandom.Value);
    if (cjson.gachaPosBias.HasValue)
      builder.SetGachaPosBias(cjson.gachaPosBias.Value.x, cjson.gachaPosBias.Value.y);
    if (cjson.gachaScaleBias.HasValue)
      builder.SetGachaScaleBias(cjson.gachaScaleBias.Value.x, cjson.gachaScaleBias.Value.y);
    if (cjson.knockingSound != null)
      if (Enum.TryParse(cjson.knockingSound, true, out ESound knockingSound))
        builder.SetKnockingSound(knockingSound);
      else {
        HadError = true;
        errors.Add($"Got invalid sound {cjson.knockingSound} for character {cjson.name}'s knocking sound.");
      }
    if (cjson.entranceTheme != null)
      if (Enum.TryParse(cjson.entranceTheme, true, out ESound entranceTheme))
        builder.SetEntranceTheme(entranceTheme);
      else {
        HadError = true;
        errors.Add($"Got invalid sound {cjson.entranceTheme} for character {cjson.name}'s entrance theme.");
      }
    if (cjson.femaRatingBonus.HasValue)
      builder.SetFEMARatingBonus(cjson.femaRatingBonus.Value);
    if (cjson.dialogCount.HasValue)
      builder.SetDialogueCount(cjson.dialogCount.Value);
    if (cjson.humanHandSprite.HasValue)
      if (processSprite(basePath, cjson.humanHandSprite.Value, out Sprite handSprite))
        builder.SetHumanHandSprite(handSprite);
      else {
        HadError = true;
        errors.Add($"Got non-existent file {cjson.humanHandSprite.Value.path} for character {cjson.name}'s human hand sprite.");
      }
    if (cjson.visitorHandSprite.HasValue)
      if (processSprite(basePath, cjson.visitorHandSprite.Value, out Sprite handSprite))
        builder.SetVisitorHandSprite(handSprite);
      else {
        HadError = true;
        errors.Add($"Got non-existent file {cjson.visitorHandSprite.Value.path} for character {cjson.name}'s visitor hand sprite.");
      }
    if (cjson.humanTeethSprite.HasValue)
      if (processSprite(basePath, cjson.humanTeethSprite.Value, out Sprite teethSprite))
        builder.SetHumanTeethSprite(teethSprite);
      else {
        HadError = true;
        errors.Add($"Got non-existent file {cjson.humanTeethSprite.Value.path} for character {cjson.name}'s human teeth sprite.");
      }
    if (cjson.visitorTeethSprite.HasValue)
      if (processSprite(basePath, cjson.visitorTeethSprite.Value, out Sprite teethSprite))
        builder.SetVisitorTeethSprite(teethSprite);
      else {
        HadError = true;
        errors.Add($"Got non-existent file {cjson.visitorTeethSprite.Value.path} for character {cjson.name}'s visitor teeth sprite.");
      }
    if (cjson.humanEyeAnimation.HasValue)
      if (processEyeAnimation(basePath, cjson.humanEyeAnimation.Value, errors, $"for character {cjson.name}'s human eye", out var value))
        builder.SetHumanEyeAnimation(value.Value.Item1, value.Value.Item2, value.Value.Item3, value.Value.Item4, value.Value.Item5, value.Value.Item6);
      else
        HadError = true;
    if (cjson.visitorEyeAnimation.HasValue)
      if (processEyeAnimation(basePath, cjson.visitorEyeAnimation.Value, errors, $"for character {cjson.name}'s visitor eye", out var value))
        builder.SetHumanEyeAnimation(value.Value.Item1, value.Value.Item2, value.Value.Item3, value.Value.Item4, value.Value.Item5, value.Value.Item6);
      else
        HadError = true;
    if (cjson.humanEarAnimation.HasValue)
      if (processMiscAnimation(basePath, cjson.humanEarAnimation.Value, errors, $"for character {cjson.name}'s human ear", out var value))
        builder.SetHumanEarAnimation(value.Value.Item1, value.Value.Item2, value.Value.Item3);
      else
        HadError = true;
    if (cjson.visitorEarAnimation.HasValue)
      if (processMiscAnimation(basePath, cjson.visitorEarAnimation.Value, errors, $"for character {cjson.name}'s visitor ear", out var value))
        builder.SetHumanEarAnimation(value.Value.Item1, value.Value.Item2, value.Value.Item3);
      else
        HadError = true;
    if (cjson.humanArmpitAnimation.HasValue)
      if (processMiscAnimation(basePath, cjson.humanArmpitAnimation.Value, errors, $"for character {cjson.name}'s human armpit", out var value))
        builder.SetHumanEarAnimation(value.Value.Item1, value.Value.Item2, value.Value.Item3);
      else
        HadError = true;
    if (cjson.visitorArmpitAnimation.HasValue)
      if (processMiscAnimation(basePath, cjson.visitorArmpitAnimation.Value, errors, $"for character {cjson.name}'s visitor armpit", out var value))
        builder.SetHumanEarAnimation(value.Value.Item1, value.Value.Item2, value.Value.Item3);
      else
        HadError = true;
    if (cjson.nightRoomSoundPath != null)
      if (processSound(basePath, cjson.nightRoomSoundPath, errors, $"for character {cjson.name}'s night sound", out AudioClip clip))
        builder.SetNightRoomSound(clip);
      else
        HadError = true;
    if (cjson.roomStates != null) {
      if (cjson.startingRoomState == null) {
        errors.Add($"Expected starting room state for character {cjson.name}.");
        HadError = true;
        foreach (KeyValuePair<string, JsonRoomState> state in cjson.roomStates) {
          if (!processRoomState(basePath, state, errors, $"for character {cjson.name}", out _))
            HadError = true;
        }
      }
      else
      {
        if (Enum.TryParse(cjson.startingRoomState, true, out ERoomPeopleState roomState))
        {
          List<RoomObjectState<ERoomPeopleState>> states = [];
          foreach (KeyValuePair<string, JsonRoomState> state in cjson.roomStates)
          {
            if (processRoomState(basePath, state, errors, $"for character {cjson.name}", out var obj))
              states.Add(obj);
            else
              HadError = true;
          }
          builder.SetPoses(states, roomState);
        }
        else {
          HadError = true;
          errors.Add($"Got invalid starting room state {cjson.startingRoomState} for character {cjson.name}");
          foreach (KeyValuePair<string, JsonRoomState> state in cjson.roomStates) {
            if (!processRoomState(basePath, state, errors, $"for character {cjson.name}", out _))
              HadError = true;
          }
        }
      }
    }
    if (Enum.TryParse(cjson.room, true, out ERoom room))
    {
      if (Enum.TryParse(cjson.roomPosition, true, out ECharacterPlace place))
      {
        builder.SetRoom(room);
        builder.SetRoomPosition(place);
      }
      else {
        HadError = true;
        errors.Add($"Got invalid position {cjson.roomPosition} for character {cjson.name}'s room position.");
      }
    }
    else {
      HadError = true;
      errors.Add($"Got invalid room {cjson.room} for character {cjson.name}'s room.");
    }
    if (HadError) {
      Log.LogError($"Encountered errors loading {cjson.name}:\n{string.Join("\n", errors).Select((v) => $"    {v}")}");
      return;
    }
    CharacterLib.AddCharacter(builder.Build());
  }

  bool processRoomState(string basePath, KeyValuePair<string, JsonRoomState> state, List<string> errors, string errorStr, out RoomObjectState<ERoomPeopleState> objectState) {
    objectState = null;
    bool hadErrors = false;
    RoomObjectState<ERoomPeopleState> ostate = new();
    if (Enum.TryParse(state.Key, true, out ERoomPeopleState estate)) {
      ostate._Name_k__BackingField = estate;
    }
    else {
      hadErrors = true;
      errors.Add($"Found invalid state {state.Key} {errorStr}.");
    }
    JsonRoomState json = state.Value;
    if (processSprite(basePath, json.sprite, out Sprite sprite)) {
      ostate._Sprite_k__BackingField = sprite;
    }
    else {
      hadErrors = true;
      errors.Add($"Got non-existent file {json.sprite.path} {errorStr}'s {state.Key} state.");
    }
    if (json.cannotInteract.HasValue)
      ostate.CannotInteract = json.cannotInteract.Value;
    ostate._PositionShift_k__BackingField = new(json.posShift.x, json.posShift.y);
    ostate._ScaleShift_k__BackingField = new(json.scaleShift.x, json.scaleShift.y);
    if (hadErrors)
      return false;
    objectState = ostate;
    return true;
  }

  private string[] GetFolderRecursively(string path)
  {
    List<string> files = [];
    foreach (string file in Directory.EnumerateFiles(path))
    {
      files.Add(file);
    }
    foreach (string directory in Directory.EnumerateDirectories(path))
    {
      files.AddRange(GetFolderRecursively(directory));
    }
    return [.. files];
  }

  bool ProcessDialog(string basePath, string charName, JsonDialogue dialogue, List<string> errors) {
    string[] possibleLocales = ["en", "ru", "zh", "zh-Hant", "fr", "de", "ja", "ko", "pt-BR", "es-AR", "es"];
    dialogue.locale ??= "en";
    if (!possibleLocales.Contains(dialogue.locale)) {
      errors.Add($"Dialogue {dialogue.path} has invalid locale {dialogue.locale}, expected any of {string.Join(", ", possibleLocales)}");
      return false;
    }
    if (dialogue.merges != null) {
      string[] files = [..dialogue.merges.Select((v) => {
        string filePath;
        if (v.StartsWith("./") || v.StartsWith("../"))
          filePath = Path.GetFullPath(v, basePath);
        else if (v.StartsWith("/"))
          filePath = Path.Join(basePath, v.TrimStart('/'));
        else
          filePath = Path.Join(basePath, v);
        return filePath;
      })];
      files = [..files.SelectMany((v) =>
      {
        if (Directory.Exists(v))
          return GetFolderRecursively(v);
        return [v];
      })];
      Array.Sort(files, (x, y) => string.Compare(Path.GetRelativePath(basePath, x), Path.GetRelativePath(basePath, y)));
      string fileContent = null;
      foreach (string file in files) {
        if (!File.Exists(file)) {
          errors.Add($"Dialogue file {file} ({charName}.{dialogue.path}, locale {dialogue.locale}) does not exist.");
          continue;
        }
        if (fileContent == null)
          fileContent = File.ReadAllText(file);
        else {
          string content = File.ReadAllText(file);
          fileContent = $"{fileContent.Trim()}\n{content.Trim()}";
        }
      }
      if (fileContent == null) {
        errors.Add($"Expected content in files for {charName}.{dialogue.path} ({dialogue.locale} locale), got no content.");
        return false;
      }
      CharacterLib.AddDialogue(dialogue.locale, $"{charName}.{dialogue.path}", fileContent);
    }
    else
    {
      string filePath;
      if (dialogue.path.StartsWith("./") || dialogue.path.StartsWith("../"))
        filePath = Path.GetFullPath(dialogue.path, basePath);
      else if (dialogue.path.StartsWith("/"))
        filePath = Path.Join(basePath, dialogue.path.TrimStart('/'));
      else
        filePath = Path.Join(basePath, dialogue.path);
      if (!File.Exists(filePath)) {
        errors.Add($"Dialogue file {filePath} ({charName}.{dialogue.path}, locale {dialogue.locale}) does not exist.");
        return false;
      }
      string fileContent = File.ReadAllText(filePath);
      CharacterLib.AddDialogue(dialogue.locale, $"{charName}.{Path.GetFileName(dialogue.path)}", fileContent);
    }
    return true;
  }

  bool processSound(string basePath, string sound, List<string> errors, string errorStr, out AudioClip? clip) {
    clip = null;
    string filePath;
    if (sound.StartsWith("./") || sound.StartsWith("../"))
      filePath = Path.GetFullPath(sound, basePath);
    else if (sound.StartsWith("/"))
      filePath = Path.Join(basePath, sound.TrimStart('/'));
    else
      filePath = Path.Join(basePath, sound);
    if (!File.Exists(filePath)) {
      errors.Add($"Sound file {filePath} {errorStr} does not exist.");
      return false;
    }
    UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(filePath, AudioType.UNKNOWN);
    uwr.SendWebRequest();
    while (!uwr.isDone) Task.Delay(1).Wait();
    clip = DownloadHandlerAudioClip.GetContent(uwr);
    uwr.Dispose();
    uwr.DisposeHandlers();
    return true;
  }

  bool processMiscAnimation(string basePath, JsonAnimation miscAnimation, List<string> errors, string errorStr, out (EAnimationCyclingType, IEnumerable<Sprite>, int)? value) {
    bool hadErrors = false;
    value = null;
    (EAnimationCyclingType, List<Sprite>, int) output = (EAnimationCyclingType.Cycle, [], 8);
    if (miscAnimation.cyclingType != null)
      if (!Enum.TryParse(miscAnimation.cyclingType, true, out output.Item1)) {
        hadErrors = true;
        errors.Add($"Got invalid cycling type {miscAnimation.cyclingType} {errorStr} animation cycling type.");
      }
    for (int i = 0; i < miscAnimation.frames.Length; i++) {
      JsonSprite jsprite = miscAnimation.frames[i];
      if (processSprite(basePath, jsprite, out Sprite sprite))
        output.Item2.Add(sprite);
      else {
        hadErrors = true;
        errors.Add($"Got non-existent file {jsprite.path} (sprite {i + 1}) {errorStr} animation frames.");
      }
    }
    if (miscAnimation.framesPerSecond.HasValue)
      output.Item3 = miscAnimation.framesPerSecond.Value;
    if (hadErrors)
      return false;
    value = output;
    return true;
  }

  bool processEyeAnimation(string basePath, JsonEyeAnimation eyeAnimation, List<string> errors, string errorStr, out (Sprite, Sprite, Ease, float, float, float)? value) {
    bool hadErrors = false;
    value = null;
    (Sprite, Sprite, Ease, float, float, float) output = (null, null, Ease.Unset, 0, 0, 0);
    if (processSprite(basePath, eyeAnimation.iris, out Sprite iris))
      output.Item1 = iris;
    else {
      hadErrors = true;
      errors.Add($"Got non-existent file {eyeAnimation.iris.path} {errorStr} iris.");
    }
    if (processSprite(basePath, eyeAnimation.white, out Sprite white))
      output.Item2 = white;
    else {
      hadErrors = true;
      errors.Add($"Got non-existent file {eyeAnimation.iris.path} {errorStr} white.");
    }
    if (!Enum.TryParse(eyeAnimation.ease, true, out output.Item3)) {
      hadErrors = true;
      errors.Add($"Got invalid ease {eyeAnimation.ease} {errorStr} easing type.");
    }
    output.Item4 = eyeAnimation.distanceMultiplier;
    output.Item5 = eyeAnimation.minMoveDuration;
    output.Item6 = eyeAnimation.maxMoveDuration;
    if (hadErrors)
      return false;
    value = output;
    return true;
  }

  bool processSprite(string basePath, JsonSprite sprite, out Sprite value) {
    value = null;
    string filePath;
    if (sprite.path.StartsWith("./") || sprite.path.StartsWith("../"))
      filePath = Path.GetFullPath(sprite.path, basePath);
    else if (sprite.path.StartsWith("/"))
      filePath = Path.Join(basePath, sprite.path.TrimStart('/'));
    else
      filePath = Path.Join(basePath, sprite.path);
    if (!File.Exists(filePath)) return false;
    if (!existingTextures.TryGetValue(filePath, out Texture2D texture))
    {
      texture = FileUtils.ReadTextureFromFile(filePath)!;
      texture.hideFlags = HideFlags.DontUnloadUnusedAsset;
      existingTextures[filePath] = texture;
    }
    Rect spriteRect;
    if (sprite.offset.HasValue) {
      if (sprite.size.HasValue) {
        spriteRect = new(sprite.offset.Value.x, sprite.offset.Value.y, sprite.size.Value.x, sprite.size.Value.y);
      }
      else {
        spriteRect = new(sprite.offset.Value.x, sprite.offset.Value.y, texture.width, texture.height);
      }
    }
    else {
      if (sprite.size.HasValue) {
        spriteRect = new(0, 0, sprite.size.Value.x, sprite.size.Value.y);
      }
      else {
        spriteRect = new(0, 0, texture.width, texture.height);
      }
    }
    Sprite outSprite;
    if (sprite.pixelsPerUnit.HasValue) {
      if (sprite.pivot.HasValue) {
        outSprite = Sprite.Create(texture, spriteRect, new(sprite.pivot.Value.x, sprite.pivot.Value.y), sprite.pixelsPerUnit.Value);
      }
      else {
        outSprite = Sprite.Create(texture, spriteRect, new(texture.width / 2, texture.height / 2), sprite.pixelsPerUnit.Value);
      }
    }
    else {
      if (sprite.pivot.HasValue) {
        outSprite = Sprite.Create(texture, spriteRect, new(sprite.pivot.Value.x, sprite.pivot.Value.y));
      }
      else {
        outSprite = Sprite.Create(texture, spriteRect, new(texture.width / 2, texture.height / 2));
      }
    }
    outSprite.hideFlags = HideFlags.DontUnloadUnusedAsset;
    value = outSprite;
    return true;
  }
}
