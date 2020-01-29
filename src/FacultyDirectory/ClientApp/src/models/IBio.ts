export interface IBio {
    bio: string;
    firstName: string;
    lastName: string;
    title: string;
    emails: string[];
    phones: string[];
    departments: string[];
    tags: string[];
    websites: IWebsite[];
}

export interface IWebsite {
  uri: string;
  title: string;
}