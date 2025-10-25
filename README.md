# NINAH.JsonCharacters

quick mod built on top of [CharacterLib](https://github.com/tairasoul/NINAH.CharacterLib) to allow for easier character creation

characters are placed within their own folder in <game folder>/BepInEx/plugins/Characters

## structure

```json
{
  "name": "CharacterName",
  // which node in your dialogue should the entrance dialogue be redirected to
  "entranceRedirect": "CharacterName_Entrance", 
  "talkRedirects": [
    // when talking to your character on a specific day, 
    // which node should we go to?
    // starts at number 2
    {
      "talkNum": 2,
      "redirected": "CharacterName_FirstTalk"
    }
  ],
  // which room your character is in
  "room": "BigRoom", 
  // where in the room your character is. 
  // if you want, you don't have to use positions that are in the room your character is, 
  // but it's recommended to keep consistency with the base game
  "roomPosition": "LivingRoomSofaRight", 
  "emotions": {
    // sprites used for specific emotions in dialogue
    "HappyEntrance": {
      "sprite": {
        "path": "./assets/entrance-sprite.png"
      }
    },
    // they can also be animated
    "Base": {
      "animation": {
        "cyclingType": "Cycle",
        "frames": [
          {
            "path": "./assets/frame1.png"
          },
          {
            "path": "./assets/frame2.png"
          }
        ],
        "framesPerSecond": 2
      }
    }
  },
  "roomStates": {
    // different states your character can be in within the room
    "Sitting": {
      "sprite": {
        "path": "./assets/sitting.png"
      },
      // how much to scale up the image
      "scaleShift": {
        "x": 4.75,
        "y": 4.75
      },
      // where to position the image
      "posShift": {
        "x": 341.7175,
        "y": -25.3564
      }
    },
    // used when your character dies as a human
    "Bags": {
      "sprite": {
        "path": "./assets/human-death.png"
      },
      "scaleShift": {
        "x": 4.75,
        "y": 4.75
      },
      "posShift": {
        "x": -53.5,
        "y": -377.5
      }
    },
    // used when your character dies as a visitor
    "Corpse": {
      "sprite": {
        "path": "./assets/visitor-death.png"
      },
      "scaleShift": {
        "x": 4.75,
        "y": 4.75
      },
      "posShift": {
        "x": -53.5,
        "y": -377.5
      }
    }
  },
  "startingRoomState": "Sitting", // which state your character starts off in
  // sprite used for the Hands sign
  "humanHandSprite": {
    "path": "./assets/hand.png"
  },
  // default visitor state of your character
  "isVisitor": false,
  // if the game should randomly choose if your character's a visitor or not
  "isRandom": false, 
  // what scale your character should render at when at the entrance
  "entranceScale": 0.4, 
  // which days this character can appear on
  "canAppearDays": [1, 2], 
  // how many times your character can be talked to when in the house
  "dialogCount": 1, 
  // your character's offset in the gacha screen
  "gachaPosBias": {
    "x": 90,
    "y": -60
  },
  // which sound to play when your character knocks at the door
  "knockingSound": "DoorKnock_4", 
  "dialog": [
    // all the files relevant to this character's dialogue
    {
      "path": "./assets/dialogue.yarn"
    }
  ]
}
```

## all properties

### name

the name of your character

### entranceRedirect

which node we should make the game go to when talking to your character at the entrance

### dialog

all files containing the dialogue for your character.

each dialogue is expected to look something like the following

```json
{
  "path": "./assets/dialogue.yarn", // path to the dialogue file
  "locale": "en" // optional, assumes en if not present
}
```

or

```json
{
  "path": "name", // name of the combined dialogue
  "merges": [
    // path to every file this merges together
    // all files listed here are appended to eachother, separated by newlines
    "./assets/split1.yarn",
    "./assets/split2.yarn"
  ],
  "locale": "en" // optional, assumes en if not present
}
```

### emotions

all emotions this character can use during dialogue.

expects something like the following

```json
{
  // can be a still sprite
  "Emotion": {
    "sprite": {
      "path": "./assets/still-sprite.png"
    }
  },
  // or animated
  "EmotionAnimated": {
    "animation": {
      // can be "Cycle", "FixLastFrame" or "PlayOnce"
      "cyclingType": "Cycle", 
      "frames": [
        // every frame of the animation
        {
          "path": "./assets/animation-frame1.png"
        }
      ],
      // how many frames to play per second, defaults to 8
      "framesPerSecond": 2 
    }
  }
}
```

### room

which room this character is in.

can be "BigRoom", "Pantry", "Office", "Kitchen", "Bathroom" or "Bedroom"

### roomPosition

where in the room this character is.

while you're not technically limited to using the positions for the character's room, it's recommended to do so to keep consistent with the base game

can be "KitchenLeftChair", "KitchenCentralChair", "KitchenStanding", "OfficeArmchair", "OfficeSofaLeft", "OfficeSofaRight", "OfficeStanding", "LivingRoomSofaLeft", "LivingRoomSofaCentral", "LivingRoomArmchair", "LivingRoomStanding", "BathroomWashingMachine", "BathroomFloorOrBath", "PantryLeftWall", "PantryRightCorner", "BedroomBed", "Huy", "LivingRoomFloor", "OfficeSofaCentral", "OfficeFloor", "LivingRoomSitting", "OfficeDresser" or "LivingRoomSofaRight"

### knockingSound

which sound to play when the character knocks on the door.

<details><summary>can be any of the following:</summary>"Provocateur_mumble", "Char_knock_medium_01", "Char_knock_hard_02", "Player_footstep_dirt_walk_03", "Sfx_phone_dial_8", "Rask_whisper", "Sfx_footstep_floor_8", "Characters_cat_meow_04", "Sfx_doorRing_3", "Maradeur_grumpy", "Characters_cat_feed_01", "Aurocam_take_photo", "Sfx_imposterScreamer_2", "Cultist_humming_02", "Char_knock_soft_03", "Consum_kombucha_03", "Emmiters_dog_bark_2", "Emmiters_dog_bark_1", "Player_footstep_wood_walk_03", "Radio_voice_loop_02", "Radio_voice_loop_03", "Radio_voice_loop_01", "Radio_voice_loop_04", "Dude_ramble", "Ambience_nature", "Sans", "Radio_notification_04", "Radio_notification_03", "Radio_notification_02", "Radio_notification_01", "House_Doors_Close_04", "Night_02_02", "Night_02_01", "Night1_reversed", "Sfx_doorKnock_1", "Sfx_doorKnock_4", "Firefighter_coughing", "Cutscenes_death_hard_knock_02", "Cutscenes_death_hard_knock_01", "Window_nail_boards_2", "Sfx_phone_dial_0", "Player_checking_wash_01", "Player_checking_wash_02", "Player_checking_wash_03", "Player_checking_wash_04", "LastBreath", "Sfx_test_mouth_3", "Curtains", "Window_knock_3", "Consum_cigarette", "Sfx_footstep_floor_6", "Char_event_special_cold_blast", "Characters_cat_feed_02", "Tv", "Char_sign_armpits_03", "Night3", "Footstep_2", "Couple_fight", "Sfx_test_hands_3", "Player_footstep_water_walk_04", "Player_footstep_water_walk_05", "Player_footstep_water_walk_06", "Player_footstep_water_walk_07", "Player_footstep_water_walk_01", "Player_footstep_water_walk_02", "Player_footstep_water_walk_03", "Player_footstep_water_walk_08", "Fridge_loop", "Player_footstep_dirt_walk_08", "Sfx_phone_dial_3", "Cold_cold", "Player_footstep_dirt_walk_05", "Window_knock_6", "Char_body_eater_move", "Gun_pick", "Player_footstep_wood_walk_04", "Sfx_doorKnock_heavy_2", "Sfx_doorKnock_heavy_1", "Ambience_house", "Basement_dig_3", "Consum_mushroom_01", "Aurocam_show_photo_2", "Char_sign_eyes_01", "Char_sign_eyes_02", "Char_sign_eyes_03", "Char_sign_eyes_04", "House_Doors_Close_02", "Footstep_1", "Char_event_child_crying_loop", "Phone_call_canceled", "Sfx_test_hands_6", "Burning_light", "Char_knock_medium_03", "Cutscenes_death_hard_step_02", "Char_sign_hands_02", "Esenin_noises", "Sfx_phone_dial_6", "Characters_cat_meow_02", "Gun_shoot", "Consum_kombucha_01", "Daughter_cry", "Consum_beer_fan", "Player_footstep_wood_walk_01", "Blind_whisper", "Gun_music", "Char_sign_armpits_04", "Death", "Vigilante", "Aurocam_show_photo_1", "Footstep_4", "Cutscenes_death_grab_neck", "Widow_cry", "Fema", "Cutscenes_death_hard_step_01", "Fridge_close", "Char_sign_hands_01", "Player_footstep_dirt_walk_02", "Sfx_phone_dial_9", "Char_event_special_teacher_suicide2", "Sfx_test_mouth_1", "DoorClose", "Window_knock_1", "Player_footstep_grass_walk_01", "Player_footstep_grass_walk_02", "Player_footstep_grass_walk_03", "Player_footstep_grass_walk_04", "Player_footstep_grass_walk_05", "Player_footstep_grass_walk_06", "Player_footstep_grass_walk_07", "Player_footstep_grass_walk_08", "Sfx_footstep_floor_4", "Sfx_phone_dial_sharp", "Sfx_imposterScreamer_5", "Fridge_open", "Consum_kombucha_04", "Noise_3_loop", "Noise_4_loop", "Noise_1_loop", "Noise_2_loop", "Twins_hum", "Player_footstep_wood_walk_02", "Characters_cat_drop_01", "Char_sign_armpits_01", "Bestson_cry", "Night1", "Wind_loop", "DoorOpen", "Char_event_special_teacher_suicide", "DoorKnock_1", "DoorKnock_3", "DoorKnock_2", "DoorKnock_4", "Ui_button_hover", "Sfx_phone_dial_1", "Sfx_test_mouth_9", "Scammer_mumbling", "Jakob_mumble", "House_Doors_Try_Open_04", "House_Doors_Try_Open_01", "House_Doors_Try_Open_03", "House_Doors_Try_Open_02", "Radio_pickup", "Player_footstep_dirt_walk_07", "Emmiters_dog_bark_distant_1", "Emmiters_dog_bark_distant_2", "Blinds", "Window_knock_4", "Fugitive_groan", "Ui_button_press", "Ambience_death", "None", "ButtonHover", "Ballerina_cry", "Super_visitor", "Characters_cat_drop_02", "Char_sign_armpits_02", "Aurocam_show_photo_4", "Night", "Footstep_3", "Sfx_test_hands_4", "Characters_cat_grab_01", "Ambience_basement", "Player_gun_shoot", "BigLebowski_cry", "Char_sign_hands_04", "Char_sign_ears_04", "Char_sign_ears_03", "Char_sign_ears_02", "Char_sign_ears_01", "FemaGuy_noises", "Sfx_phone_dial_4", "Char_sign_teeth_02", "Cutscenes_death_player_fear", "Player_footstep_dirt_walk_04", "Dream_bodyeater_whisperdoor_02", "Emmiters_dog_growling_2", "Emmiters_dog_growling_1", "Luka_talk", "End_screen", "Window_knock_7", "Foreigner_talk", "Doc_mumbilng", "WifeFema_mumble", "Cutscenes_death_door_breaking", "Player_gun_pickup_02", "Player_footstep_wood_walk_07", "Sfx_phone_hangDown", "Consum_postcard", "Basement_dig_2", "Night4", "Consum_mushroom_02", "Main_menu", "Radio_put_down", "Aurocam_show_photo_3", "Char_screamer_03", "House_Doors_Close_03", "Footstep_6", "Char_knock_medium_02", "Char_sign_hands_03", "Char_knock_hard_03", "Sfx_phone_dial_7", "Char_sign_teeth_01", "Characters_cat_meow_03", "Player_gun_shoot_2", "Nun_pray", "Sfx_phone_pickup", "ButtonClick", "Sfx_footstep_floor_2", "Dream_bodyeater_whisperdoor_01", "Day", "Hunter_humming", "Emmiters_crow_1", "Emmiters_crow_2", "Sfx_sirenAlarm_LOOP", "Sfx_imposterScreamer_3", "Cultist_humming_03", "Emmiters_fox_yell_1", "Emmiters_fox_yell_2", "Char_knock_soft_02", "Consum_kombucha_02", "House_Doors_Open_01", "House_Doors_Open_02", "House_Doors_Open_03", "House_Doors_Open_04", "Player_gun_pickup_01", "Sfx_getToPeepHole_2", "Emmiters_dog_howl_1", "Gun_pick_2", "Characters_cat_meow_MAU", "NotTrue_laugh", "Char_death_idle", "Char_body_eater_idle", "Whispers1", "Whispers3", "Whispers2", "Footstep_5", "Buddy_cry", "Window_nail_boards_3", "Sfx_phone_dial_star", "Blinds_close", "Player_footstep_dirt_walk_01", "Char_sign_teeth_04", "Anxiety_mumble", "Window_knock_2", "Sfx_footstep_floor_5", "GraveDigger_mumble", "Cutscenes_death_break_neck", "Sfx_typewriterTest", "Curtains_close", "Player_footstep_wood_walk_08", "Char_knock_soft_01", "Dream", "FortuneTeller_hum", "Char_screamer_04", "Night2", "Mushroom_eater", "Credits", "Teacher_cry", "Char_knock_hard_04", "Sfx_phone_dial_2", "Char_event_special_ballerina_01", "Char_event_special_ballerina_02", "Char_event_special_ballerina_03", "Char_knock_medium_04", "Player_footstep_dirt_walk_06", "Dream_bodyeater_whisperdoor_04", "Window_knock_5", "Wheelchair_crazy", "Radio_noise_loop", "Player_footstep_wood_walk_05", "Char_knock_soft_04", "Characters_cat_purr_4", "Characters_cat_purr_2", "Characters_cat_purr_3", "Characters_cat_purr_1", "Anger_fury", "Char_screamer_01", "House_Doors_Close_01", "Consum_cigarette_2", "Mother_snoring", "Characters_cat_grab_02", "Consum_beer", "Char_knock_hard_01", "Sfx_phone_dial_5", "Fan_burping", "Char_sign_teeth_03", "Characters_cat_meow_01", "TaxiDriver_coughing", "Dream_bodyeater_whisperdoor_03", "Sfx_test_eye_4", "Window_knock_8", "Cultist_humming_01", "Char_sign_auracam_01", "Window_knock_long_loop", "Drift", "Player_footstep_wood_walk_06", "Prophet", "Theorist_mumbling", "Fatman_breath", "Day_02", "Basement_dig_1", "Sfx_phone_busy", "Night5", "Phone_call_beep", "Greatmother_talking", "Wolfhound_disrespect", "Char_event_child_found_02", "Char_event_child_found_03", "Char_event_child_found_01", "Char_event_child_found_04", "Sfx_phone_dial_tone", "Edgar_cry", "Char_screamer_02", "Cultists", "Window_nail_boards_1"</details>

### entranceTheme

what sound to play in the background when viewing this character through the peephole.

same as above

### talkRedirects

a list of which days we should redirect to which nodes

expects the following

```json
{
  // which day to redirect for. 
  // starts off at 2 and increments by 1 for each day
  "talkNum": 2,
  // the node to redirect to
  "redirected": "Char_TalkOne"
}
```

### emotionRedirects

which emotions used in your dialogue to redirect to actual game emotions

added incase you use one of the many character-specific emotions for something else and want a more appropriate name in your dialogue

### roomStates

states the character can be in when in their room

expects something like the following

```json
{
  "State": {
    // the sprite used for this state
    "sprite": {
      "path": "./assets/sprite.png"
    },
    // how much to change this sprite's scale
    "scaleShift": {
      "x": 4.75,
      "y": 4.75
    },
    // where to put the sprite
    "posShift": {
      "x": 341.7175,
      "y": -25.3564
    }
  }
}
```

available states are "FullSize", "Staying", "Sitting", "Something", "Corpse", "Lying", "Pregnant1", "Pregnant2", "Pregnant3", "Bags", "Twisted", "Hanged", "Burned", "LostChild", "LostChildCorpse" and "Vomit"

### startingRoomState

which state this character starts out in when entering the house

same states as above

### humanTeethSprite

the sprite used for the Teeth sign when this character is a human.

expects something like the following

```json
{
  "path": "./assets/humanteeth.png", // path to texture file
  // how far to offset (in pixels) from the top left in the texture file
  "offset": {
    "x": 100,
    "y": 100
  },
  // size of the sprite in pixels
  "size": {
    "x": 100,
    "y": 100
  },
  // unsure what this does honestly
  "pivot": {
    "x": 0.5,
    "y": 0.5
  },
  // how many pixels are equal to 1 unity unit for this sprite
  "pixelsPerUnit": 100
}
```

### visitorTeethSprite

the sprite used for the Teeth sign when this character is a visitor.

same as above.

### humanHandSprite

the sprite used for the Hands sign when this character is a human.

same as above.

### visitorHandSprite

the sprite used for the Hands sign when this character is a visitor.

same as above.

### humanEarAnimation

the animation used for the Ears sign when this character is a human.

expects something like the following

```json
{
  // can be "Cycle", "FixLastFrame" or "PlayOnce"
  "cyclingType": "Cycle",
  "frames": [
    // every frame of the animation
    {
      "path": "./assets/animation-frame1.png"
    }
  ],
  // how many frames to play per second, defaults to 8
  "framesPerSecond": 2 
}
```

### visitorEarAnimation

the animation used for the Ears sign when this character is a visitor.

same as above.

### humanArmpitAnimation

the animation used for the Armpits sign when this character is a human.

same as above.

### visitorArmpitAnimation

the animation used for the Armpits sign when this character is a visitor.

same as above.

### humanEyeAnimation

the animation used for the Eyes sign when this character is a human.

expects something like the following

```json
{
  // the iris of the eye
  "iris": {
    "path": "./assets/iris.png" // see humanTeethSprite for reference
  },
  // the eye white
  "white": {
    "path": "./assets/white.png" // see humanTeethSprite for reference
  },
  // easing type. 
  // see https://easings.net/ for the available easing types 
  // + Linear, just remove the ease prefix for the name
  "ease": "Linear", 
  // how much to multiply the eye distance when moving it.
  // recommended to keep relatively low, higher numbers will go out of the frame
  "distanceMultiplier": 0.5, 
  // minimum duration for the eye to move in seconds
  "minMoveDuration": 1,
  // maximum duration for the eye to move in seconds
  "maxMoveDuration": 2
}
```

### visitorEyeAnimation

the animation used for the Eyes sign when this character is a visitor.

same as above.

### isVisitor

whether or not this character's default state is being a visitor.

### nightRoomSoundPath

the path to an audio file for the sound played at night at this character's room.

### corpseMaskPath

the path to a black and white image which determines what area to pixelate on the corpse state.

the white part of the image will be pixelated.

### femaRatingBonus

cannot determine what this does from the code, assumedly a bonus used when picking characters to take when fema arrives

### minimumCompletionsToAppear

how many times the player should have beaten the game before the character appears

### gachaPosBias

the position offset for the character in the gacha screen

expects the following

```json
{
  "x": 50,
  "y": -60
}
```

### gachaScaleBias

the scale offset for the character in the gacha screen

expects the following

```json
{
  "x": 1,
  "y": 1
}
```

### dialogPosition

have not tested this, assumedly where o nthe screen the dialog box is

expects the following

```json
{
  "x": -268,
  "y": 10
}
```

### dialogScale

how much to scale up/down the dialog box

### dialogCount

how many days your character can be talked to for

### canAppearDays

which days this character can appear on

expects something like the following

```json
[1, 5, 8]
```

### isRandom

whether or not this character should have a randomized visitor state

### entranceScale

how much to scale up/down the character at the entrance