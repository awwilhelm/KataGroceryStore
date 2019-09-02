## Assumptions
1. APIs use the same flow as REST APIs, however I can call it by a function instead of an endpoint
2. Since we aren't using a database, I am going to use the tests to run the program (aka I'm not mocking endpoints)
3. In normal grocery stores items are displayed in order of being scanned.  
It never specified this, so I will assume it can be displayed in any order
4. It does not specify if duplicate promo codes are accepted, so I am assuming they are
5. Since this is a grocery store and we know shoppers can only have the amount of items in the store
and that number is less tha 250,000 performance doesn't matter (aka not worth the time to optimize)
6. I am assuming that all specials are store-wide and not like a coupon that a customer
must hand over
7. Although this is a grocery scanner, it doesn't specify if this app only supports one transaction
or multiple.  I am assuming that it is only one transaction and to start a new one you must restart
the app.
8. I made the assumption you can only scan one item at a time and not three apples, but if there is a
special for three apples, it will apply when the third one is scanned
9. I haven't seen any requirements on supporting a product to have more than one markdown at a time so I will
not be supporting that.  I will be supporting multiple markdowns on separate products though (even though it didn't
specify this, it makes sense to implement)


## Notes along the way
1. I ran into the problem with removing items, should I modify the current total or should I just recalculate.
The pros of modifying it are that it has higher performance and cons are that constantly modifying data can be dangerous
especially if you don't have one source of truth.  Even though that's how I have it programmed this app will evolve and possibly
get to a point where its not maintainable.  Where as the performance is negligible since n (number of items in cart) is a finite number

2. At first I was going to do all of the tests then write all of the code, but then I remembered about vertical slicing and 
having a completed product at each iteration so that if I don't have enough time, I still have something to show for.

3. If this was a RESTFUL API I would return proper error codes like 400, 500 etc. when errors occur instead of just
passing the raw error 

4. Since there are no ids it is sometimes impossible to remove the exact one you want to remove, except when there is weight

5. Just because there are "best practices" doesn't mean there are "always follow best practices".  I like to follow
the approach where you make the simpliest product and refactor as the size increases.  I find a lot of OO methodolgies
don't always make sense in small projects like this.  The benefit of ditching some of those best practices makes
my code easy to follow and less files to jump around. (Note: It does get unmanagible when you scale the size of this
project)

6. Not exactly sure what this means "Markdown prices must always be used in favor of the per-unit price during the sale.
There are laws protecting the customer from false advertising."