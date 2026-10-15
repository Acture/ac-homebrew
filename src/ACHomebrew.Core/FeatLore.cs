using System.Collections.Generic;

namespace ACHomebrew.Feats
{
    /// <summary>One-line flavour text shown under the family line of newer feats, keyed by blueprint name.</summary>
    internal static class FeatLore
    {
        private static readonly Dictionary<string, (string en, string zh)> Lore = new()
        {
            ["CounterAttack"] = (Common.Text("Text.FeatLore.Labels.Text", "You treat an enemy's swing not as an assault to weather, but as an open invitation to strike.", false),
                Common.Text("Text.FeatLore.Labels.Text", "你从不消极招架来袭的锋芒，而是将每一次受击都视作递剑的契机。", true)),
            ["MissCounter"] = (Common.Text("Text.FeatLore.Labels.Text.2", "When an adversary's strike cuts empty air, you slip past their recovered guard before they can reset their stance.", false),
                Common.Text("Text.FeatLore.Labels.Text.2", "当敌人的兵刃斩入空处，你便在对方重整架势之前欺身而入。", true)),
            ["Punisher"] = (Common.Text("Text.FeatLore.Labels.Text.3", "A blow aimed at you leaves a vivid trace in your mind, guiding your weapon back toward its author with cold purpose.", false),
                Common.Text("Text.FeatLore.Labels.Text.3", "落在身上的每一记刀兵都在你心中铭刻下轨迹，指引你将同等的痛楚如数奉还。", true)),
            ["ParryCrit"] = (Common.Text("Text.FeatLore.Labels.Text.4", "You read the subtle tremor along an opponent's guard at the moment of deflection, guiding your reply into their exposed core.", false),
                Common.Text("Text.FeatLore.Labels.Text.4", "你在兵刃相交的震颤中洞悉对手力道的去向，顺着其破绽送出致命一击。", true)),
            ["Shove"] = (Common.Text("Text.FeatLore.Labels.Text.5", "You lean your entire bulk into the friction of combat, ensuring every clash sends the opponent staggering backward.", false),
                Common.Text("Text.FeatLore.Labels.Text.5", "你将躯干的浑厚力道灌注于每一次碰撞，令对手在攻防交错间身形失据、步步踉跄。", true)),
            ["ReturnBlow"] = (Common.Text("Text.FeatLore.Labels.Text.6", "Your flesh meets incoming weapons like cold iron, shuddering hard enough under impact to splinter the hands that strike you.", false),
                Common.Text("Text.FeatLore.Labels.Text.6", "你的身躯沉硬如淬火寒铁，来袭的劲道未曾撼动你分毫，反而将震荡反逼回敌人肺腑。", true)),
            ["KrakenShell"] = (Common.Text("Text.FeatLore.Labels.Text.7", "You weather blows like the armored hide of a deep-sea horror, sloughing off crippling afflictions as the pain crests.", false),
                Common.Text("Text.FeatLore.Labels.Text.7", "你如深海巨兽般承受风暴般的击打，在痛楚达到顶点时将其化作蜕除一切滞碍的生机。", true)),
            ["KillingSpree"] = (Common.Text("Text.FeatLore.Labels.Text.8", "Each felled foe frees your motion rather than slows it, pulling you eagerly toward the next engagement.", false),
                Common.Text("Text.FeatLore.Labels.Text.8", "每一个倒下的敌人都令你的呼吸与步伐更加轻快，推着你毫无停歇地扑向下一处战团。", true)),
            ["BattleRhythm"] = (Common.Text("Text.FeatLore.Labels.Text.9", "You treat a melee as a continuous melody of strikes, sharpening your edge with every hit so long as your defense remains unsullied.", false),
                Common.Text("Text.FeatLore.Labels.Text.9", "你将搏杀编织成一曲不绝的韵律，只要身形不滞、寸步未伤，攻势便如层波叠浪般渐入化境。", true)),
            ["Fervor"] = (Common.Text("Text.FeatLore.Labels.Text.10", "By fixing your gaze entirely upon a single opponent, you read their evasions in real time and weave extra strikes into their blind spots.", false),
                Common.Text("Text.FeatLore.Labels.Text.10", "你将全副心神死死咬住眼前的敌手，在紧逼的脚步与虚实交错中觅得更快的空隙。", true)),
            ["CrushingRhythm"] = (Common.Text("Text.FeatLore.Labels.Text.11", "You structure your attacks like verse, building steady pressure until the fourth beat breaks the defense entirely.", false),
                Common.Text("Text.FeatLore.Labels.Text.11", "你的招式如四段起伏的战鼓，前三击层层蓄势铺垫，终击如奔雷破空，直摧敌穴。", true)),
            ["CorrosiveFinish"] = (Common.Text("Text.FeatLore.Labels.Text.12", "You know how to strike where flesh has already torn, widening an existing agony into a fatal collapse.", false),
                Common.Text("Text.FeatLore.Labels.Text.12", "你精于顺着敌人已被撕裂的旧伤下刃，将最初的破口化作无法止歇的溃败。", true)),
            ["PhoenixFury"] = (Common.Text("Text.FeatLore.Labels.Text.13", "When your own blood spills, you channel the searing burn of mortal distress into a furnace that engulfs everything near you.", false),
                Common.Text("Text.FeatLore.Labels.Text.13", "当鲜血从伤口涌出，你将濒死的痛楚化为焚尽四方的炽热烈焰。", true)),
            ["Feast"] = (Common.Text("Text.FeatLore.Labels.Text.14", "You strike not merely against armor, but against the vital momentum that keeps towering monsters upright.", false),
                Common.Text("Text.FeatLore.Labels.Text.14", "你的刀锋不只斩破外甲，更直指维系庞大身躯立于大地之上的原初生机。", true)),
            ["FeastCurrent"] = (Common.Text("Text.FeatLore.Labels.Text.15", "Your first engagements aim to shatter an opponent's pristine vigor before they have settled into the rhythm of the duel.", false),
                Common.Text("Text.FeatLore.Labels.Text.15", "你在敌阵初交时悍然出手，专挫其最盛的血气，令其未及交锋便折损大半锐气。", true)),
            ["FeastSpell"] = (Common.Text("Text.FeatLore.Labels.Text.16", "You shape your spells to unravel the underlying weave that binds an immense creature's vital essence together.", false),
                Common.Text("Text.FeatLore.Labels.Text.16", "你的咒语旨在瓦解维系庞大造物存在的本源脉络，使其宏伟的生命力随法术共振而崩解。", true)),
            ["FeastNatural"] = (Common.Text("Text.FeatLore.Labels.Text.17", "With tooth and claw, you draw upon the ancient law of the hunt, devouring an adversary's vitality to renew your own.", false),
                Common.Text("Text.FeatLore.Labels.Text.17", "你的爪牙依循荒野最原始的掠食律法，撕扯猎物的庞大生机以补给己身的气血。", true)),
            ["Lifesteal"] = (Common.Text("Text.FeatLore.Labels.Text.18", "You align your weapon strokes to draw vigor from spilled life, turning every successful cut into renewed stamina.", false),
                Common.Text("Text.FeatLore.Labels.Text.18", "你的锋刃浸透了掠夺气血的隐秘诀窍，使每一次切开皮肉的交锋都化为滋养身躯的甘霖。", true)),
            ["SlotHarvest"] = (Common.Text("Text.FeatLore.Labels.Text.19", "As a foe's breath departs, you gather the unraveling magical filaments of their dying consciousness to rekindle your own reserves.", false),
                Common.Text("Text.FeatLore.Labels.Text.19", "当敌人的生机溃散之时，你随手收拢其将散未散的精神余波，重新点亮脑海中耗竭的法位。", true)),
            ["ArcaneOrb"] = (Common.Text("Text.FeatLore.Labels.Text.20", "With your deeper spell slots preserved intact, your simplest cantrips inherit the radiant pressure of an overflowing mind.", false),
                Common.Text("Text.FeatLore.Labels.Text.20", "当充盈的法术之海平稳沉寂时，哪怕最微小的戏法也会承载起深潭涌流般的磅礴威能。", true)),
            ["EssenceFlux"] = (Common.Text("Text.FeatLore.Labels.Text.21", "You weave incantations so cleanly that stray echoes of power fold back into your mental repertoire rather than dispersing into the air.", false),
                Common.Text("Text.FeatLore.Labels.Text.21", "你的施法精细入微，溢散的魔力残响得以循着法理逆流回神识，令耗损的灵光失而复得。", true)),
            ["ManaBreak"] = (Common.Text("Text.FeatLore.Labels.Text.22", "You hone your weapon to disrupt the subtle concentration of spellcasters, sundering their memory of spells with each ringing blow.", false),
                Common.Text("Text.FeatLore.Labels.Text.22", "你的兵刃专为截断精神脉络而铸，每一击都震荡着施法者的识海，生生撕裂其准备好的咒文。", true)),
            ["ManaBreakSpell"] = (Common.Text("Text.FeatLore.Labels.Text.23", "You direct your magic to pierce an opponent's prepared reservoir, turning the strain of their expended intellect into a concussive rupture.", false),
                Common.Text("Text.FeatLore.Labels.Text.23", "你的法术直接贯入敌方施法者的识海孔隙，以其枯竭的精神空腔为引，引发剧烈的反噬崩鸣。", true)),
            ["ReStealth"] = (Common.Text("Text.FeatLore.Labels.Text.24", "In the confusion born of a sudden, brutal wound, you dissolve into the periphery of sight before onlookers can locate your blade.", false),
                Common.Text("Text.FeatLore.Labels.Text.24", "你在致命一击撕开的慌乱与阴影中从容隐没，令旁观者在目击惨剧的瞬间便丢失了你的行踪。", true)),
            ["LoneWolf"] = (Common.Text("Text.FeatLore.Labels.Text.25", "Having trained without companions to shield your flanks, your vigilance sharpens only when the ground around you is clear.", false),
                Common.Text("Text.FeatLore.Labels.Text.25", "习惯了孤身行走于险地，唯有在身周空无一人时，你的身手与感官才得以舒展至极致。", true)),
            ["EssenceShift"] = (Common.Text("Text.FeatLore.Labels.Text.26", "Through hundreds of life-or-death skirmishes, your muscles absorb the evasive reflexes of every agile prey you run down.", false),
                Common.Text("Text.FeatLore.Labels.Text.26", "历经百战的厮杀磨砺，你将所斩猎物的避险本能与轻灵身段，潜移默化地烙入自己的骨髓。", true)),
            ["FleshHeap"] = (Common.Text("Text.FeatLore.Labels.Text.27", "Walking beside heaps of fallen enemies thickens your sinews and bones, packing your frame with stubborn, brutal mass.", false),
                Common.Text("Text.FeatLore.Labels.Text.27", "越过堆积如山的倒毙强敌，弥散的死战凶煞重塑了你的筋骨，使你的身躯愈发沉重而不可撼动。", true)),
            ["ArcaneSiphon"] = (Common.Text("Text.FeatLore.Labels.Text.28", "By dissecting the final patterns of dying arcanists, you piece together new insights that expand the architecture of your thought.", false),
                Common.Text("Text.FeatLore.Labels.Text.28", "你在击杀施法者时辨析其未散的精神回路，将敌人的玄思与法理化为拓宽自身思绪的基石。", true)),
            ["DevouredVigor"] = (Common.Text("Text.FeatLore.Labels.Text.29", "Each life cut short before you leaves a flicker of vital warmth that settles into your marrow, steadily fortifying your survival.", false),
                Common.Text("Text.FeatLore.Labels.Text.29", "每一个断绝于你眼前的生灵，都留有一缕游离的热力浸润你的肺腑，让你的生机日见充盈。", true)),
            ["Necromastery"] = (Common.Text("Text.FeatLore.Labels.Text.30", "You bind the vengeful remnants of slain foes to your wake, compelling their restless sorrow to sharpen your next blow.", false),
                Common.Text("Text.FeatLore.Labels.Text.30", "你将倒下之人的残魂拘束于衣袂之间，令其不得安歇的怨念附着于兵刃之上，化为森然杀意。", true)),
            ["SpiritLink"] = (Common.Text("Text.FeatLore.Labels.Text.31", "You interweave your vital humors with the conjured forms at your side, letting restorative medicine mend you as one organism.", false),
                Common.Text("Text.FeatLore.Labels.Text.31", "你将自身的血脉气机与召唤物勾连如一，使得任何抚慰创痛的甘露都能在主从之间流转共享。", true)),
            ["ShortBlade"] = (Common.Text("Text.FeatLore.Labels.Text.32", "Understanding that long steel demands wide arcs, you practice burying compact blades into tight crevices where polearms falter.", false),
                Common.Text("Text.FeatLore.Labels.Text.32", "兵刃越短，越能于乱战微隙间穿行自如；你精研贴身走锋之法，在长兵无法挥洒处直刺要害。", true)),
            ["CloseQuarters"] = (Common.Text("Text.FeatLore.Labels.Text.33", "You crowd inside your opponent's ideal guard, turning what should be a lack of weapon reach into an oppressive, suffocating press.", false),
                Common.Text("Text.FeatLore.Labels.Text.33", "你以敏捷的短步挤占对手引以为傲的攻守间距，将本属劣势的短寸化作令敌窒息的压制。", true)),
            ["CastingStat_Str"] = (Common.Text("Text.FeatLore.Labels.Text.34", "You bypass abstract scholastic formulas, tearing spell formulas into reality through sheer bodily exertion and vocal thunder.", false),
                Common.Text("Text.FeatLore.Labels.Text.34", "你摈弃繁杂精微的冥想，单凭沉雄的身躯与如雷的断喝，生生将法术脉络按入现世。", true)),
            ["CastingStat_Dex"] = (Common.Text("Text.FeatLore.Labels.Text.35", "Your somatic components mimic the blinding precision of a master embroiderer, threading magical currents through agile fingertips.", false),
                Common.Text("Text.FeatLore.Labels.Text.35", "你的施法手势宛如穿针引线的飞梭，以神乎其技的指尖轻灵，自如牵引灵动的法力丝线。", true)),
            ["CastingStat_Con"] = (Common.Text("Text.FeatLore.Labels.Text.36", "You treat your own bloodstream and lungs as the crucible for magic, anchoring spells to the unyielding stamina of your flesh.", false),
                Common.Text("Text.FeatLore.Labels.Text.36", "你将自身的血气与脏腑视作盛载咒文的洪炉，用充盈而百炼不摧的体魄托举起法术的重压。", true)),
            ["ResourceStat_Str"] = (Common.Text("Text.FeatLore.Labels.Text.37", "You draw deep upon your physical bulk, converting sheer muscular mass into an endurance that outlasts lesser bodies.", false),
                Common.Text("Text.FeatLore.Labels.Text.37", "你的战意深深依托于雄厚的气力，将浑身精肉沉淀为源源不绝的底力蓄水池。", true)),
            ["ResourceStat_Dex"] = (Common.Text("Text.FeatLore.Labels.Text.38", "You preserve energy through frictionless movement, allowing balance and reflex to fuel your special techniques without fatigue.", false),
                Common.Text("Text.FeatLore.Labels.Text.38", "你借毫无滞涩的流转动作积蓄势能，使轻巧灵动的体态成为战法施展的不竭源泉。", true)),
            ["ResourceStat_Con"] = (Common.Text("Text.FeatLore.Labels.Text.39", "Your vitality runs deep into your bones, ensuring that moments requiring extraordinary resolve never find you emptied.", false),
                Common.Text("Text.FeatLore.Labels.Text.39", "你的体魄深沉如渊，即使在需要爆发全部潜能的关头，那股自血肉中涌出的本源也绝不枯竭。", true)),
        };

        // Inserts the lore line after the first (family) line of a description.
        public static (string en, string zh) Apply(string name, (string en, string zh) desc)
        {
            if (!Lore.TryGetValue(name, out var lore)) return desc;
            return (Insert(desc.en, Common.Text($"{name}.Lore", lore.en)), Insert(desc.zh, Common.Text($"{name}.Lore", lore.zh, true)));
        }

        private static string Insert(string text, string lore)
        {
            var end = text.IndexOf('\n');
            return end < 0 ? lore + "\n\n" + text : text.Substring(0, end) + "\n" + lore + text.Substring(end);
        }
    }
}
