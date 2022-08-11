import React, { useEffect } from 'react';
import { Redirect } from 'react-router-dom';

import { ISitePerson } from '../models/ISitePerson';
import { Loading } from './Loading';

interface IPersonRecord {
  isAdmin: boolean;
  sitePeople: ISitePerson[];
}

// reads user info and redirects to the appropriate page
export const Landing = () => {
  const [redirect, setRedirect] = React.useState<string>();

  useEffect(() => {
    const getSelf = async () => {
      const results: IPersonRecord = await fetch(
        'api/faculty/userinfo'
      ).then(r => r.json());

      if (results.isAdmin) {
        setRedirect('/people');
      } else if (results.sitePeople && results.sitePeople.length > 0) {
        // we do have a siteperson, so redirect to pronunication page for now
        // if we come up with more functions for a site person, we can make a site person landing page
        setRedirect('/pronunciation/' + results.sitePeople[0].id);
      } else {
        // if they aren't an admin and don't have a siteperson, redirect to not allowed page
        setRedirect('/error403');
      }
    };

    getSelf();
  }, []);

  if (redirect) {
    return <Redirect to={redirect} />;
  }

  return <Loading text='LOADING...'></Loading>;
};
