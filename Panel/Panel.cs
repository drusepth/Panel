using System;
using System.Collections.Generic;

// A Panel's goal is to recruit Panelists that must come to a conclusion based
// upon the randomly-assigned subset of traits each Panelist prefers. This
// conclusion may be complete opinion or it may be fact, depending on the 
// traits allowed, but with a large enough sample size (num_of_panelists) and
// number of possible traits, a statistical representation of what real people
// would think about a given object can be formed.

// Examples of traits, when looking at a painting to decide whether it is
// "pretty" or not, may include:
//  - A preference for color over monochromatic shades, penalizing the use
//    of greys and rewarding the use of more vibrant colors
//  - A preference for monochromatic shades, penalizing the use of color
//    and rewarding the use of only black and white
//  - A preference for curved lines, rewarding straight lines and penalizing
//    the use of curves, or vice versa
// etc.

// Example usage:
//
// Panel jury = new Panel(13, 5);
// jury.AddTrait(preferColors);
// jury.AddTrait(preferMonochrome);
// jury.AddTrait(preferCurvedLines);
// jury.AddTrait(preferStraightLines);
// double likeability = jury.Opine(some_object);
// bool verdict = jury.Verdict(some_object);

namespace Panel
{
    // Functions that determine how much an object is "liked" should have one
    // parameter (the object to look at) and return a double within the range
    // -100 to 100, where -100 is extreme dislike and 100 is an extreme like.
    public delegate double LikeabilityFunction(Object o);

    // A panel consists of a group of Panelists that act to make a collective
    // decision upon whether they like an object or not.
    public class Panel
    {
        private int panelist_count;
        private int trait_count;
        private List<Trait> possible_traits = new List<Trait>();
        private List<Panelist> panelists = new List<Panelist>();

        public Panel(int num_of_panelists, int traits_per_panelist)
        {
            panelist_count = num_of_panelists;
            trait_count = traits_per_panelist;
        }

        // Recruit panelists
        private void RecruitPanelists()
        {
            panelists.Clear();

            for (int i = 0; i < panelist_count; i++)
            {
                panelists.Add(new Panelist(this, trait_count));
            }
        }

        // Allows developers to add traits that will be combined into 
        // personalities.
        // f - A function to run on Object o and return a numerical amount
        //     representing how much this trait would favor the given object,
        //     on a scale of -100 to 100.
        public void AddTrait(LikeabilityFunction func)
        {
            possible_traits.Add(new Trait(func));
        }

        // Based on the preferences of the members of this panel, come to a
        // collective decision upon how much this object is liked or disliked.
        public double Opine(Object o)
        {
            RecruitPanelists();

            // Through excellent negotiation, not a single member of the panel
            // is stubborn and they all share their views. As such, the 
            // likeability of the object is the average across all panelists.
            double average_likeability = 0;
            foreach (Panelist panelist in Panelists) 
            {
                average_likeability += panelist.Opine(o);
            }
            return average_likeability / Panelists.Count;
        }

        // Rather than a sliding scale between -100 and 100, the members of
        // the panel must decide on a black or white decision upon whether 
        // this object is "liked" or "disliked". A likeable object will return
        // true, while one that is disliked will return false.
        public bool Verdict(Object o)
        {
            RecruitPanelists();

            // The decision is positive if at least 50% of the panel will
            // agree that the object is liked.
            int positive_votes = 0;
            foreach (Panelist panelist in Panelists)
            {
                if (panelist.Verdict(o))
                    positive_votes++;
            }

            return (positive_votes / Panelists.Count) > 0.50;
        }

        public List<Trait> Traits { get { return possible_traits; } }
        public List<Panelist> Panelists { get { return panelists; } }
    }

    // Panelists are created from some random subset of traits that have been
    // predefined, in order to prevent any bias in whether any trait would
    // overpower the others.
    public class Panelist
    {
        private Random RNG;
        private Panel belongs_to;
        private List<Trait> traits = new List<Trait>();

        public Panelist(Panel panel, int num_traits)
        {
            RNG = new Random();
            belongs_to = panel;

            // Copy over this List so we can easily avoid duplicate traits
            List<Trait> potential_traits = new List<Trait>(panel.Traits);

            // Ensure we're not trying to add more traits than available
            if (num_traits > potential_traits.Count - 1)
            {
                num_traits = potential_traits.Count - 1;
            }

            // Assign random traits to this panelist
            for (int i = 0; i < num_traits; i++)
            {
                int t = RNG.Next(potential_traits.Count);
                traits.Add(potential_traits[t]);
                potential_traits.RemoveAt(t);
            }
        }

        // Taking in to account each of the panelist's traits, they must 
        // assign a "likeability" number to an object, representing how "good"
        // it is, or how much they like it.
        public double Opine(Object o)
        {
            double average_likeness = 0;
            foreach (Trait trait in Traits)
            {
                average_likeness += trait.Inspect(o);
            }
            return average_likeness / Traits.Count;
        }

        // Determines in a strict yes (true) or no (false) fashion whether
        // this panelist "likes" Object o. In a truly neutral situation
        // where the panelist has absolutely no preference, they are marked
        // in the negative.
        public bool Verdict(Object o)
        {
            return Opine(o) > 0;
        }

        public List<Trait> Traits { get { return traits; } }
    }

    // Traits are nothing more than preferences. They should be defined by some
    // function written by the developer using this framework to look at an
    // object and assign some value of "likeableness" within the range of -100
    // to 100. 
    public class Trait
    {
        LikeabilityFunction foo;

        public Trait(LikeabilityFunction func)
        {
            foo = func;
        }

        // Evaluate the LikeabilityFunction on the given object and return the
        // result, then ensure it's within valid limits.
        public double Inspect(Object bar) 
        {
            double result = foo(bar);
            return Bound(result);
        }

        // Ensure a result is within the bounds of [-100, 100].
        public double Bound(double raw)
        {
            if (raw < -100)
                return -100;
            else if (raw > 100)
                return 100;
            else
                return raw;
        }
    }
}
