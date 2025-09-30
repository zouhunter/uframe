using System;
using System.Collections;
using System.Collections.Generic;
using UFrame;

public class AppFacade : FacadeTemplate<AppFacade, string, string, string>
{
    protected override Facade<string,string,string> CreateFacade()
    {
        return base.CreateFacade();
    }
}
