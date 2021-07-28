OLD="BabiesAndChildren"
grep -rli "$OLD" * | xargs -i@ sed -i "s/$OLD/BabiesAndChildren/g" @
