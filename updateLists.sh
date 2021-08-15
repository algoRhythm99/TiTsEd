#!/bin/bash

thisScript="$0"
parent="$(dirname $(readlink -fm "${thisScript}"))"

DocBase="$(dirname "${parent}")"
OutputBase="${parent}/DataLists"
SourceFolder="${DocBase}/TiTS/scripts"
TiTsEdData="${parent}/TiTsEd/TiTsEd.Data.xml"

current_time=$(date "+%Y.%m.%d-%H.%M.%S")
newFolder="${OutputBase}/${current_time}"
mkdir -p "${newFolder}"


# part flags
declare -a partFlags=("Arm"
                      "Cock"
                      "Ear"
                      "Face"
                      "Leg"
                      "Skin"
                      "Tail"
                      "Tongue"
                      "vagoo"
                     )
if [ 1 == 1 ]; then
    for bFlag in "${partFlags[@]}"
    do
        echo "Checking ${bFlag} flags"
        propName="${bFlag}Flag"
        rawFile="${newFolder}/${propName}s.raw.txt"
        newFile="${newFolder}/${propName}s.txt"
        sortedFile="${newFolder}/${propName}s.sorted.txt"
        regex='('${propName}'.*GLOBAL.FLAG.*)'
        echo regex="$regex"
        echo >$rawFile
        find $SourceFolder -name "*.as" -exec grep -Poe "$regex" {} \; >>$rawFile
        cat $rawFile | sort -u >$newFile
        regex="s/.*\((GLOBAL.FLAG[[:alnum:]_]+)\).*/\1/"
        echo regex="$regex"
        sed -i -r -e "$regex" $newFile
        regex="^$"
        echo regex="$regex"
        sed -i -r -e "/$regex/d" $newFile
        cat $newFile | sort -u >$sortedFile
        regex="^GLOBAL.FLAG_NAMES$"
        echo regex="$regex"
        sed -i -r -e "/$regex/d" $sortedFile
    done
fi


declare -a parts=("antennae"
                  "arm"
                  "c"
                  "ear"
                  "eye"
                  "face"
                  "horn"
                  "leg"
                  "tail"
                  "tongue"
                  "vagina"
                 )
if [ 1 == 1 ]; then
    for bType in "${parts[@]}"
    do
        echo "Checking ${bType} types"
        propName="${bType}Type"
        rawFile="${newFolder}/${propName}s.raw.txt"
        newFile="${newFolder}/${propName}s.txt"
        sortedFile="${newFolder}/${propName}s.sorted.txt"
        regex='('${propName}'.*GLOBAL.TYPE.*)'
        echo regex="$regex"
        echo >$rawFile
        find $SourceFolder -name "*.as" -exec grep -Poe "$regex" {} \; >>$rawFile
        cat $rawFile | sort -u >$newFile
        regex="s/.*(GLOBAL.TYPE[[:alnum:]_]+).*/\1/"
        echo regex="$regex"
        sed -i -r -e "$regex" $newFile
        regex="^$"
        echo regex="$regex"
        sed -i -r -e "/$regex/d" $newFile
        cat $newFile | sort -u >$sortedFile
        regex="^GLOBAL.TYPE_NAMES$"
        echo regex="$regex"
        sed -i -r -e "/$regex/d" $sortedFile
    done
fi


# colors
declare -a colors=("fur"
                   "hair"
                   "lip"
                   "nipple"
                   "vagina"
                  )
if [ 1 == 1 ]; then
    for bColor in "${colors[@]}"
    do
        echo "Checking ${bColor} colors"
        propName="${bColor}Color"
        rawFile="$newFolder/${propName}s.raw.txt"
        newFile="${newFolder}/${propName}s.txt"
        sortedFile="${newFolder}/${propName}s.sorted.txt"
        regex=${propName}".*"
        echo regex="$regex"
        echo >$rawFile
        find $SourceFolder -name "*.as" -exec grep -Poe "$regex" {} \; >>$rawFile
        cat $rawFile | sort -u >$newFile
        regex="s/.*[=!][[:space:]]*(.*)[;]/\1/"
        echo regex="$regex"
        sed -i -r -e "$regex" $newFile
        regex="^$"
        echo regex="$regex"
        sed -i -r -e "/$regex/d" $newFile
        cat $newFile | sort -u >$sortedFile
    done
fi


# hairStyle
if [ 0 == 1 ]; then
    echo "Checking hairStyles"
    propName="hairStyle"
    rawFile="$newFolder/${propName}s.raw.txt"
    newFile="$newFolder/${propName}s.txt"
    sortedFile="${newFolder}/${propName}s.sorted.txt"
    regex='\.'${propName}'.*'
    echo regex="$regex"
    echo >$rawFile
    find $SourceFolder -name "*.as" -exec grep -Poe "$regex" {} \; >>$rawFile
    cat $rawFile | sort -u >$newFile
    regex="/.*"${propName}"[[:space:]]*[=!]+.*/!d"
    echo regex="$regex"
    sed -i -r -e "$regex" $newFile
    regex="s/.*[=!]+[[:space:]]*(.+)[;]/\1/"
    echo regex="$regex"
    sed -i -r -e "$regex" $newFile
    regex="^$"
    echo regex="$regex"
    sed -i -r -e "/$regex/d" $newFile
    cat $newFile | sort -u >$sortedFile
fi


# skinTone
if [ 1 == 1 ]; then
    echo "Checking skinTones"
    propName="skinTone"
    rawFile="$newFolder/${propName}s.raw.txt"
    newFile="$newFolder/${propName}s.txt"
    sortedFile="${newFolder}/${propName}s.sorted.txt"
    regex='\.'${propName}'.*'
    echo regex="$regex"
    echo >$rawFile
    find $SourceFolder -name "*.as" -exec grep -Poe "$regex" {} \; >>$rawFile
    cat $rawFile | sort -u >$newFile
    regex="/.*"${propName}"[[:space:]]*[=!]+.*/!d"
    echo regex="$regex"
    sed -i -r -e "$regex" $newFile
    regex="s/.*[=!]+[[:space:]]*(.+)[;]/\1/"
    echo regex="$regex"
    sed -i -r -e "$regex" $newFile
    regex="^$"
    echo regex="$regex"
    sed -i -r -e "/$regex/d" $newFile
    cat $newFile | sort -u >$sortedFile
fi


# Key items
if [ 1 == 1 ]; then
    echo "Checking keyItems"
    propName="KeyItem"
    rawFile="$newFolder/${propName}s.raw.txt"
    newFile="$newFolder/${propName}s.txt"
    sortedFile="$newFolder/${propName}s.sorted.csv"
    regex='.*\.(create|has)KeyItem\((["[:alnum:] ,_-]+)\).*'
    echo regex="$regex"
    echo >$rawFile
    find $SourceFolder -name "*.as" -exec grep -Poe "$regex" {} \; >>$rawFile
    cat $rawFile | sort -u >$newFile
    sed -i -r -e "s/$regex/\2/" $newFile
    regex='[,][[:space:]]*["]["]$'
    echo regex="$regex"
    sed -i -r -e "s/$regex//g" $newFile
    regex='[,][[:space:]]*0$'
    echo regex="$regex"
    for i in {1..4}; do
        sed -i -r -e "s/$regex//g" $newFile
    done
    regex='^couponName$'
    echo regex="$regex"
    sed -i -r -e "/$regex/d" $newFile
    regex='^["][[:alnum:]]["]$'
    echo regex="$regex"
    sed -i -r -e "/$regex/d" $newFile
    regex="^$"
    echo regex="$regex"
    sed -i -r -e "/$regex/d" $newFile
    cat $newFile | sort -u >$sortedFile
    sed -r -e 's/["]([[:alnum:] _-]+)["].*/\1/;t;d' $sortedFile | sort -u >$newFile
fi


# Perks
if [ 1 == 1 ]; then
    echo "Checking perks"
    propName="Perk"
    rawFile="$newFolder/${propName}s.raw.txt"
    newFile="$newFolder/${propName}s.txt"
    sortedFile="$newFolder/${propName}s.sorted.csv"
    regex='.*\.(create|has)Perk\((["[:alnum:] ,_-]+)\).*'
    echo regex="$regex"
    echo >$rawFile
    find $SourceFolder -name "*.as" -exec grep -Poe "$regex" {} \; >>$rawFile
    cat $rawFile | sort -u >$newFile
    sed -i -r -e "s/$regex/\2/" $newFile
    regex='[,][[:space:]]*["]["]$'
    echo regex="$regex"
    sed -i -r -e "s/$regex//g" $newFile
    regex='[,][[:space:]]*0$'
    echo regex="$regex"
    for i in {1..4}; do
        sed -i -r -e "s/$regex//g" $newFile
    done
    regex='^["][[:alnum:]]["]$'
    echo regex="$regex"
    sed -i -r -e "/$regex/d" $newFile
    regex="^$"
    echo regex="$regex"
    sed -i -r -e "/$regex/d" $newFile
    cat $newFile | sort -u >$sortedFile
    sed -r -e 's/["]([[:alnum:] _-]+)["].*/\1/;t;d' $sortedFile | sort -u >$newFile
fi


# Status Effects
if [ 1 == 1 ]; then
    echo "Checking statusEffects"
    propName="StatusEffect"
    rawFile="$newFolder/${propName}s.raw.txt"
    newFile="$newFolder/${propName}s.txt"
    sortedFile="$newFolder/${propName}s.sorted.csv"
    regex='.*\.(create|has)StatusEffect\((["[:alnum:] ,_-]+)\).*'
    echo regex="$regex"
    echo >$rawFile
    find $SourceFolder -name "*.as" -exec grep -Poe "$regex" {} \; >>$rawFile
    cat $rawFile | sort -u >$newFile
    sed -i -r -e "s/$regex/\2/" $newFile
    regex='[,][[:space:]]*["]["]$'
    echo regex="$regex"
    sed -i -r -e "s/$regex//g" $newFile
    regex='[,][[:space:]]*0$'
    echo regex="$regex"
    for i in {1..4}; do
        sed -i -r -e "s/$regex//g" $newFile
    done
    regex='^["][[:alnum:]]["]$'
    echo regex="$regex"
    sed -i -r -e "/$regex/d" $newFile
    regex="^$"
    echo regex="$regex"
    sed -i -r -e "/$regex/d" $newFile
    cat $newFile | sort -u >$sortedFile
    sed -r -e 's/["]([[:alnum:] _-]+)["].*/\1/;t;d' $sortedFile | sort -u >$newFile
fi

# flags
if [ 1 == 1 ]; then
    echo "Checking flags"
    propName="flag"
    rawFile="$newFolder/${propName}s.raw.txt"
    newFile="$newFolder/${propName}s.txt"
    sortedFile="${newFolder}/${propName}s.sorted.txt"
    regex='.*flags\[(["][[:alnum:] ,_-]+["])\].*'
    echo regex="$regex"
    echo >$rawFile
    find $SourceFolder -name "*.as" -exec grep -Poe "$regex" {} \; >>$rawFile
    cat $rawFile | sort -u >$newFile
    sed -i -r -e "s/$regex/\1/" $newFile
    regex='["]'
    echo regex="$regex"
    sed -i -r -e "s/$regex//g" $newFile
    regex="^9999$"
    echo regex="$regex"
    sed -i -r -e "/$regex/d" $newFile
    regex="^9999_placeholder$"
    echo regex="$regex"
    sed -i -r -e "/$regex/d" $newFile
    regex="^.$"
    echo regex="$regex"
    sed -i -r -e "/$regex/d" $newFile
    regex="^prop$"
    echo regex="$regex"
    sed -i -r -e "/$regex/d" $newFile
    regex="^$"
    echo regex="$regex"
    sed -i -r -e "/$regex/d" $newFile
    cat $newFile | sort -u >$sortedFile

    if [ -f $sortedFile ]; then
        newXmlFile="$newFolder/missingFlags.xml"
        while read f; do
            # echo Checking for flag "$f"
            if ! grep -q "\"$f\"" "$TiTsEdData"; then
                echo "Does not have flag $f"
                echo '		<Flag Name="'$f'" />' >>$newXmlFile
            fi
        done <$sortedFile
    fi

fi


hasItem() {
    TiTsEdData="$1"
    SourceFolder="$3"
    fn="$0"
    dir="$(dirname $fn)"
    bn="$(basename $fn)"
    fne="${bn%%.*}"
    classPackage="$(echo $dir|sed -e "s#${SourceFolder}##")"
    classPackage="${classPackage%/}"
    classPackage="${classPackage#/}"
    classPackage="$(echo $classPackage|sed -e "s#/#.#g")"
    classPackage="${classPackage/items/Items}"
    itemClass="${classPackage}::${fne}"
    regex='"'${itemClass}'"'
    grepOutput=$(grep -F $regex "$TiTsEdData">/dev/null 2>/dev/null)
    grepExit=$?

    missingItemsFile="$4"
    itemsXMLFile="$5"

    hasShieldDefense=0
    hasFortification=0
    hasShields=0
    hasFields=0

    missingText="$(printf '%s\n' "$fn")"

    itemType=$(perl -ne '/(this[.])?type\s*=\s*(GLOBAL[.][\w]+)\s*;/ && print "\n$2";' "$fn")
    missingText="${missingText}$(printf 'itemType=\n%s' "$itemType")"

    shortName=$(perl -ne '/(this[.])?shortName\s*=\s*"([\w'"\\\'"'“”’‘, &+|:!_.\\(\\)\"-]+)"\s*;/ && print "\n$2";' "$fn")
    missingText="${missingText}$(printf 'shortName=\n%s' "$shortName")"

    longName=$(perl -ne '/(this[.])?longName\s*=\s*"([\w'"\\\'"'“”’‘, &+|:!_.\\(\\)\"-]+)"\s*;/ && print "\n$2";' "$fn")
    missingText="${missingText}$(printf 'longName=\n%s' "$longName")"

    stackSize=$(perl -ne '/(this[.])?stackSize\s*=\s*([\d]+)\s*;/ && print "\n$2";' "$fn")
    if [ -z "${stackSize}" ]; then
        stackSize=1
    fi
    missingText="${missingText}$(printf 'stackSize=\n%s' "$stackSize")"

    fortification=$(perl -ne '/(this[.])?fortification\s*=\s*([-]?[\d]+)\s*;/ && print "\n$2";' "$fn")
    missingText="${missingText}$(printf 'fortification=\n%s' "$fortification")"

    shieldDefense=$(perl -ne '/(this[.])?shieldDefense\s*=\s*([-]?[\d]+)\s*;/ && print "\n$2";' "$fn")
    missingText="${missingText}$(printf 'shieldDefense=\n%s' "$shieldDefense")"

    shields=$(perl -ne '/(this[.])?shields\s*=\s*([-]?[\d]+)\s*;/ && print "\n$2";' "$fn")
    missingText="${missingText}$(printf 'shields=\n%s' "$shields")"

    itemXML="$(printf '\n\t\t\t<Item Stack="%s" Name="%s" ID="%s"' "$stackSize" "$shortName" "$itemClass" )"
    if [ ! -z "${LongName}" ]; then
        itemXML="${itemXML}$(printf ' LongName="%s"' "$longName")"
    fi
    #if [ ! -z "${stackSize}" ]; then
    #    itemXML="${itemXML}$(printf ' Stack="%s"' "$stackSize")"
    #fi
    if [ ! -z "${fortification}" ] && [ "$fortification" != "0" ]; then hasFortification=1; hasFields=1; fi
    if [ ! -z "${shieldDefense}" ] && [ "$shieldDefense" != "0" ]; then hasShieldDefense=1; hasFields=1; fi
    if [ ! -z "${shields}" ] && [ "$shields" != "0" ]; then hasShields=1; hasFields=1; fi

    if [ $hasFields -eq 1 ]; then
        itemXML="${itemXML}$(printf '>\n')"
        if [ $hasFortification -eq 1 ]; then
            itemXML="${itemXML}$(printf '\n\t\t\t\t<ItemField Name="fortification" Type="int" Value="%s" />\n' "$fortification")"
        fi
        if [ $hasShieldDefense -eq 1 ]; then
            itemXML="${itemXML}$(printf '\n\t\t\t\t<ItemField Name="shieldDefense" Type="int" Value="%s" />\n' "$shieldDefense")"
        fi
        if [ $hasShields -eq 1 ]; then
            itemXML="${itemXML}$(printf '\n\t\t\t\t<ItemField Name="shields" Type="int" Value="%s" />\n' "$shields")"
        fi
        itemXML="${itemXML}$(printf '\n\t\t\t</Item>\n')"
    else
        itemXML="${itemXML}$(printf ' />\n')"
    fi
    itemXML="${itemXML}$(printf '\n')"

    echo "$itemXML" >>$itemsXMLFile

    if [ ! $grepExit -eq 0 ]; then
        echo "$missingText" >>$missingItemsFile
        echo "$itemXML" >>$missingItemsFile
        echo "" >>$missingItemsFile
        echo "" >>$missingItemsFile
    fi
}
export -f hasItem


if [ 1 == 1 ]; then
    newFile="$newFolder/missingItems.txt"
    itemsFile="$newFolder/items.xml"
    echo "Checking for missing items"
    find $SourceFolder/classes/items -name "*.as" -exec bash -c "hasItem \"$TiTsEdData\" \"$0\" \"$SourceFolder\" \"$newFile\" \"$itemsFile\"" {} \;
fi

if [ 0 == 1 ]; then
    echo "Checking for CodexEntries"
    newXmlFile="$newFolder/CodexEntries.xml"
    perl -pe 's#CodexManager[.]addCodexEntry[(]\s*CodexManager[.]CODEX_TYPE_([\w_]+)\s*,\s*["](.+)["]\s*,\s*["](.+)["]\s*,\s*.+[)]#<CodexEntry Type="\1" Group="\2"><!\[CDATA\[\3\]\]></CodexEntry>\r\n#g;' $SourceFolder/classes/TiTS.as >$newXmlFile
fi
